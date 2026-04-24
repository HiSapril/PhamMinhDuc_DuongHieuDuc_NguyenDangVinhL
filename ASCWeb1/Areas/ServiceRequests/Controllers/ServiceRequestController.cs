using ASC.Business.Interfaces;
using ASC.Utilities;
using ASC.Model;
using ASC.Model.Models;
using ASCWeb1.Areas.ServiceRequests.Models;
using ASCWeb1.Controllers;
using ASCWeb1.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ASCWeb1.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;

        public ServiceRequestController(IServiceRequestOperations operations, IMapper mapper, IMasterDataCacheOperations masterData)
        {
            _serviceRequestOperations = operations;
            _mapper = mapper;
            _masterData = masterData;
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            var masterData = await _masterData.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
            return View(new NewServiceRequestViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                var masterData = await _masterData.GetMasterDataCacheAsync();
                ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
                ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
                return View(request);
            }

            // Map the view model to Azure model
            var serviceRequest = _mapper.Map<NewServiceRequestViewModel, ServiceRequest>(request);
            // Set RowKey, PartitionKey, RequestedDate, Status properties
            var userDetails = HttpContext.User.GetCurrentUserDetails();
            serviceRequest.PartitionKey = userDetails?.Email ?? string.Empty;
            serviceRequest.RowKey = Guid.NewGuid().ToString();
            serviceRequest.RequestedDate = request.RequestedDate;
            serviceRequest.Status = Status.New.ToString();

            // Set Audit properties
            serviceRequest.CreatedBy = userDetails?.Name ?? "System";
            serviceRequest.UpdatedBy = userDetails?.Name ?? "System";
            serviceRequest.CreatedDate = DateTime.UtcNow;
            serviceRequest.UpdatedDate = DateTime.UtcNow;

            await _serviceRequestOperations.CreateServiceRequestAsync(serviceRequest);
            return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });
        }
    }
}

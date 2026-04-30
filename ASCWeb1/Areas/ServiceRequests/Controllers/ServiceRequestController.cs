using ASC.Business.Interfaces;
using ASC.Utilities;
using ASC.Model;
using ASC.Model.Models;
using ASCWeb1.Areas.ServiceRequests.Models;
using ASCWeb1.Controllers;
using ASCWeb1.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using ASCWeb1.Hubs;

namespace ASCWeb1.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    [Authorize]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IServiceRequestMessageOperations _serviceRequestMessageOperations;
        private readonly IHubContext<ServiceMessagesHub> _signalRConnectionManager;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;
        private readonly UserManager<IdentityUser> _userManager;

        public ServiceRequestController(
            IServiceRequestOperations operations,
            IServiceRequestMessageOperations messageOperations,
            IHubContext<ServiceMessagesHub> signalRConnectionManager,
            IMapper mapper,
            IMasterDataCacheOperations masterData,
            UserManager<IdentityUser> userManager)
        {
            _serviceRequestOperations = operations;
            _serviceRequestMessageOperations = messageOperations;
            _signalRConnectionManager = signalRConnectionManager;
            _mapper = mapper;
            _masterData = masterData;
            _userManager = userManager;
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

        [HttpGet]
        public async Task<IActionResult> ServiceRequestDetails(string rowKey, string partitionKey)
        {
            if (string.IsNullOrEmpty(rowKey) || string.IsNullOrEmpty(partitionKey))
            {
                return RedirectToAction("Dashboard", "Dashboard");
            }

            // Get service request from database
            var serviceRequestDetails = await _serviceRequestOperations.GetServiceRequestByRowKey(partitionKey, rowKey);
            
            if (serviceRequestDetails == null)
            {
                return RedirectToAction("Dashboard", "Dashboard");
            }

            // Access Check
            var userDetails = HttpContext.User.GetCurrentUserDetails();
            if (HttpContext.User.IsInRole(Roles.Engineer.ToString()) &&
                serviceRequestDetails.ServiceEngineer != userDetails?.Email)
            {
                return Forbid();
            }

            if (HttpContext.User.IsInRole(Roles.User.ToString()) &&
                serviceRequestDetails.PartitionKey != userDetails?.Email)
            {
                return Forbid();
            }

            // Get audit details
            var serviceRequestAuditDetails = await _serviceRequestOperations.GetServiceRequestAuditByPartitionKey(partitionKey);

            // Get master data for dropdowns
            var masterData = await _masterData.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
            ViewBag.Status = Enum.GetValues(typeof(Status)).Cast<Status>().Select(v => v.ToString()).ToList();
            ViewBag.ServiceEngineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());

            // Map to ViewModel
            var viewModel = _mapper.Map<ServiceRequest, UpdateServiceRequestViewModel>(serviceRequestDetails);

            return View(new ServiceRequestDetailViewModel
            {
                ServiceRequest = viewModel,
                ServiceRequestAudit = serviceRequestAuditDetails.OrderByDescending(p => p.UpdatedDate).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateServiceRequestDetails(UpdateServiceRequestViewModel serviceRequest)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("ServiceRequestDetails", new { rowKey = serviceRequest.RowKey, partitionKey = serviceRequest.PartitionKey });
            }

            var originalServiceRequest = await _serviceRequestOperations.GetServiceRequestByRowKey(serviceRequest.PartitionKey, serviceRequest.RowKey);
            
            if (originalServiceRequest == null)
            {
                return RedirectToAction("Dashboard", "Dashboard");
            }

            // Update Requested Services - Allow all users to update this field
            originalServiceRequest.RequestedServices = serviceRequest.RequestedServices;

            // Update Status only if user role is either Admin or Engineer
            // Or Customer can update the status if it is only in Pending Customer Approval.
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()) ||
                HttpContext.User.IsInRole(Roles.Engineer.ToString()) ||
                (HttpContext.User.IsInRole(Roles.User.ToString()) && originalServiceRequest.Status == Status.PendingCustomerApproval.ToString()))
            {
                originalServiceRequest.Status = serviceRequest.Status;

                // Update Service Engineer field only if user role is Admin
                if (HttpContext.User.IsInRole(Roles.Admin.ToString()))
                {
                    originalServiceRequest.ServiceEngineer = serviceRequest.ServiceEngineer;
                }
            }

            // Update audit fields for all updates
            var userDetails = HttpContext.User.GetCurrentUserDetails();
            originalServiceRequest.UpdatedBy = userDetails?.Name ?? "System";
            originalServiceRequest.UpdatedDate = DateTime.UtcNow;

            _serviceRequestOperations.UpdateServiceRequest(originalServiceRequest);
            
            return RedirectToAction("ServiceRequestDetails", new { rowKey = serviceRequest.RowKey, partitionKey = serviceRequest.PartitionKey });
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequestMessages(string serviceRequestId)
        {
            var messages = await _serviceRequestMessageOperations.GetServiceRequestMessageAsync(serviceRequestId);
            return Json(messages.OrderBy(m => m.MessageDate).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> CreateServiceRequestMessage([FromBody] CreateServiceRequestMessageViewModel messageViewModel)
        {
            try
            {
                Console.WriteLine("=== CreateServiceRequestMessage called ===");
                Console.WriteLine($"User authenticated: {User.Identity?.IsAuthenticated}");
                Console.WriteLine($"User name: {User.Identity?.Name}");
                
                if (messageViewModel == null)
                {
                    Console.WriteLine("ERROR: messageViewModel is null");
                    return BadRequest("Message view model is null");
                }

                Console.WriteLine($"PartitionKey: {messageViewModel.PartitionKey}");
                Console.WriteLine($"FromDisplayName: {messageViewModel.FromDisplayName}");
                Console.WriteLine($"FromEmail: {messageViewModel.FromEmail}");
                Console.WriteLine($"Message: {messageViewModel.Message}");

                if (string.IsNullOrEmpty(messageViewModel.Message))
                {
                    Console.WriteLine("ERROR: Message is empty");
                    return BadRequest("Invalid message");
                }

                // Create ServiceRequestMessage from ViewModel
                var message = new ServiceRequestMessage
                {
                    PartitionKey = messageViewModel.PartitionKey,
                    RowKey = Guid.NewGuid().ToString(),
                    FromDisplayName = messageViewModel.FromDisplayName,
                    FromEmail = messageViewModel.FromEmail,
                    Message = messageViewModel.Message,
                    MessageDate = DateTime.UtcNow,
                    CreatedBy = messageViewModel.FromDisplayName ?? "System",
                    UpdatedBy = messageViewModel.FromDisplayName ?? "System",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                Console.WriteLine($"Created message with RowKey: {message.RowKey}");

                // Save to database
                await _serviceRequestMessageOperations.CreateServiceRequestMessageAsync(message);
                Console.WriteLine("Message saved to database");

                // Send SignalR notification to all clients in the group
                await _signalRConnectionManager.Clients.Group(message.PartitionKey)
                    .SendAsync("ReceiveMessage", message.FromDisplayName, message.FromEmail, message.Message);
                Console.WriteLine("SignalR notification sent");

                Console.WriteLine("=== CreateServiceRequestMessage completed successfully ===");
                return Ok(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR in CreateServiceRequestMessage ===");
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

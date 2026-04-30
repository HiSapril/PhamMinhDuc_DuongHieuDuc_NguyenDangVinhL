using ASC.Business.Interfaces;
using ASC.Model;
using ASC.Model.Models;
using ASC.Utilities;
using ASCWeb1.Areas.ServiceRequests.Models;
using ASCWeb1.Configuration;
using ASCWeb1.Controllers;
using ASCWeb1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ASCWeb1.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class DashboardController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMasterDataCacheOperations _masterData;

        public DashboardController(IServiceRequestOperations operations, IMasterDataCacheOperations masterData)
        {
            _serviceRequestOperations = operations;
            _masterData = masterData;
        }

        public async Task<IActionResult> Dashboard()
        {
            // List of Status which were to be queried.
            var status = new List<string>
            {
                Status.New.ToString(),
                Status.InProgress.ToString(),
                Status.Initiated.ToString(),
                Status.RequestForInformation.ToString()
            };

            List<ServiceRequest> serviceRequests = new List<ServiceRequest>();
            List<ServiceRequest> recentUpdates = new List<ServiceRequest>();
            Dictionary<string, int> activeServiceRequests = new Dictionary<string, int>();
            
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()))
            {
                serviceRequests = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(DateTime.UtcNow.AddYears(-1), status);
                
                // Admin can see ALL service requests in Recent Updates (no status filter)
                recentUpdates = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(DateTime.UtcNow.AddYears(-1));
                
                // Get Active Service Requests (InProgress and Initiated) grouped by Service Engineer
                var serviceEngineerServiceRequests = await _serviceRequestOperations.GetActiveServiceRequests(
                    new List<string>
                    {
                        Status.InProgress.ToString(),
                        Status.Initiated.ToString()
                    });
                
                if (serviceEngineerServiceRequests.Any())
                {
                    activeServiceRequests = serviceEngineerServiceRequests
                        .GroupBy(x => x.ServiceEngineer)
                        .ToDictionary(p => p.Key, p => p.Count());
                }
            }
            else if (HttpContext.User.IsInRole(Roles.Engineer.ToString()))
            {
                serviceRequests = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(
                        DateTime.UtcNow.AddDays(-7),
                        status,
                        serviceEngineerEmail: HttpContext.User.GetCurrentUserDetails()?.Email ?? string.Empty);
                
                // Engineer can see their assigned requests in Recent Updates
                recentUpdates = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(
                        DateTime.UtcNow.AddDays(-7),
                        serviceEngineerEmail: HttpContext.User.GetCurrentUserDetails()?.Email ?? string.Empty);
            }
            else
            {
                serviceRequests = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(DateTime.UtcNow.AddYears(-1),
                        email: HttpContext.User.GetCurrentUserDetails()?.Email ?? string.Empty);
                
                // User can see their own requests in Recent Updates
                recentUpdates = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(DateTime.UtcNow.AddYears(-1),
                        email: HttpContext.User.GetCurrentUserDetails()?.Email ?? string.Empty);
            }

            return View(new DashboardViewModel
            {
                ServiceRequests = serviceRequests.OrderByDescending(p => p.RequestedDate).ToList(),
                RecentUpdates = recentUpdates.OrderByDescending(p => p.UpdatedDate).Take(10).ToList(),
                ActiveServiceRequests = activeServiceRequests
            });
        }
    }
}

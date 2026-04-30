using ASC.DataAccess.Interfaces;
using ASC.Model.Models;
using ASC.Model.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Business.Interfaces
{
    public class ServiceRequestOperations : IServiceRequestOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceRequestOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateServiceRequestAsync(ServiceRequest request)
        {
            await _unitOfWork.Repository<ServiceRequest>().AddAsync(request);
            _unitOfWork.CommitTransaction();
        }

        public ServiceRequest UpdateServiceRequest(ServiceRequest request)
        {
            _unitOfWork.Repository<ServiceRequest>().Update(request);
            _unitOfWork.CommitTransaction();
            return request;
        }

        public async Task<ServiceRequest> UpdateServiceRequestStatusAsync(string rowKey, string partitionKey, string status)
        {
            var serviceRequest = await _unitOfWork.Repository<ServiceRequest>().FindAsync(partitionKey, rowKey);
            if (serviceRequest == null)
                throw new NullReferenceException();
            serviceRequest.Status = status;
            _unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
            _unitOfWork.CommitTransaction();
            return serviceRequest;
        }
        public async Task<List<ServiceRequest>> GetServiceRequestsByRequestedDateAndStatus(
    DateTime? requestedDate, List<string>? status = null, string email = "", string serviceEngineerEmail = "")
        {
            var query = Queries.GetDashboardQuery(requestedDate, status, email, serviceEngineerEmail);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByQuery(query);
            return serviceRequests.ToList();
        }

        public async Task<ServiceRequest?> GetServiceRequestByRowKey(string partitionKey, string rowKey)
        {
            return await _unitOfWork.Repository<ServiceRequest>().FindAsync(partitionKey, rowKey);
        }

        public async Task<List<ServiceRequest>> GetActiveServiceRequests(List<string> status)
        {
            var query = Queries.GetDashboardQuery(null, status, "", "");
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByQuery(query);
            return serviceRequests.Where(sr => !string.IsNullOrEmpty(sr.ServiceEngineer)).ToList();
        }

        public async Task<List<ServiceRequest>> GetServiceRequestAuditByPartitionKey(string partitionKey)
        {
            // For audit, we query by PartitionKey which contains the audit trail
            // In a real scenario, you might have a separate audit table
            // For now, we'll return the service request history
            var allRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllAsync();
            return allRequests.Where(sr => sr.PartitionKey == partitionKey || sr.PartitionKey.StartsWith(partitionKey + "-")).ToList();
        }
    }
}

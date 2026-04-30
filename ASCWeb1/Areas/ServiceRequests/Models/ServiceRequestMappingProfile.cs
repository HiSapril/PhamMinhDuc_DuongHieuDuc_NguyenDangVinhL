using ASC.Model.Models;
using AutoMapper;

namespace ASCWeb1.Areas.ServiceRequests.Models
{
    public class ServiceRequestMappingProfile : Profile
    {
        public ServiceRequestMappingProfile()
        {
            CreateMap<ServiceRequest, NewServiceRequestViewModel>();
            CreateMap<NewServiceRequestViewModel, ServiceRequest>();
            CreateMap<ServiceRequest, UpdateServiceRequestViewModel>();
            CreateMap<UpdateServiceRequestViewModel, ServiceRequest>();
        }
    }
}

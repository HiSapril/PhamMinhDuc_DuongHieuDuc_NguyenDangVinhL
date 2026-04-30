using AutoMapper;
using ASC.Model.Models;
using ASCWeb1.Areas.ServiceRequests.Models;

namespace ASCWeb1.Areas.Configuration.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MasterDataKey, MasterDataKeyViewModel>();
            CreateMap<MasterDataKeyViewModel, MasterDataKey>();
            CreateMap<MasterDataValue, MasterDataValueViewModel>();
            CreateMap<MasterDataValueViewModel, MasterDataValue>();

            // ServiceRequest <-> ViewModel mappings
            CreateMap<NewServiceRequestViewModel, ServiceRequest>()
                .ForMember(dest => dest.RowKey, opt => opt.Ignore())
                .ForMember(dest => dest.PartitionKey, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceEngineer, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedDate, opt => opt.Ignore());

            CreateMap<ServiceRequest, UpdateServiceRequestViewModel>()
                .ForMember(dest => dest.VehicleName, opt => opt.MapFrom(src => src.VehicleName))
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.VehicleType))
                .ForMember(dest => dest.RequestedServices, opt => opt.MapFrom(src => src.RequestedServices))
                .ForMember(dest => dest.RequestedDate, opt => opt.MapFrom(src => src.RequestedDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ServiceEngineer, opt => opt.MapFrom(src => src.ServiceEngineer))
                .ForMember(dest => dest.RowKey, opt => opt.MapFrom(src => src.RowKey))
                .ForMember(dest => dest.PartitionKey, opt => opt.MapFrom(src => src.PartitionKey));

            CreateMap<UpdateServiceRequestViewModel, ServiceRequest>()
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedDate, opt => opt.Ignore());
        }
    }
}

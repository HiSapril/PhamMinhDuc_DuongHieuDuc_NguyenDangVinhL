using AutoMapper;
using ASC.Model.Models;

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
        }
    }
}

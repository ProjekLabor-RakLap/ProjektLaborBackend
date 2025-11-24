using AutoMapper;
using ProjectLaborBackend.Dtos.UserDTOs;
using ProjectLaborBackend.Entities;

namespace ProjectLaborBackend.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile() 
        {
            CreateMap<UserRegisterDTO, User>()
                .ForMember(dest => dest.Role, opt => opt.Ignore());
            CreateMap<UserLoginDTO, User>();
            CreateMap<UserPatchDTO, User>()
                //.ForMember(x => x.Email, opt => opt.Ignore())
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ForgotUserPutPasswordDTO, User>();
            CreateMap<UserPutPasswordDTO, User>().ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.NewPassword));
            CreateMap<User, UserGetDTO>().ForMember(dest => dest.WarehouseIds, opt => opt.MapFrom(src => src.Warehouses.Select(w => w.Id)));
        }
    }
}

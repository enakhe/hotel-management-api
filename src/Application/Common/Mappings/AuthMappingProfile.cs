using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManagement.Application.Auth.Commands;
using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Domain.Entities.Data;

namespace HotelManagement.Application.Common.Mappings;
public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<RegisterCommand, RegisterUserDto>();

        CreateMap<RegisterUserDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<LoginCommand, LoginRequestDto>();
    }
}

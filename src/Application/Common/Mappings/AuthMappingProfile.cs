using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Core.Auth.Commands;
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

        CreateMap<LoginRequestDto, LoginCommand>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));

        CreateMap<ChangePasswordCommand, ChangePasswordDto>()
            .ForMember(dest => dest.CurrentPassword, opt => opt.MapFrom(src => src.CurrentPassword))
            .ForMember(dest => dest.NewPassword, opt => opt.MapFrom(src => src.NewPassword));

        CreateMap<RequestPasswordResetCommand, ResetPasswordRequestDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        CreateMap<ConfirmPasswordResetCommand, ResetPasswordConfirmDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.Token))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));
    }
}

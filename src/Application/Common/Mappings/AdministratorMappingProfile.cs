using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Domain.Entities.Administrator;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Entities.Data;

namespace HotelManagement.Application.Common.Mappings;
public class AdministratorMappingProfile : Profile
{
    public AdministratorMappingProfile()
    {
        // User mappings
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch!.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.MiddleName} {src.LastName}"))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        CreateMap<CreateUserDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId))
                //.ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => null)) // Assuming no profile picture is set initially
                //.ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password)) // Password will be hashed in the service layer
                .ForMember(dest => dest.SecurityStamp, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpperInvariant()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email.ToUpperInvariant()))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

        CreateMap<UpdateUserDto, ApplicationUser>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId))
            .ForMember(dest => dest.LastUpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        // Role mappings
        CreateMap<ApplicationRole, RoleDto>()
            .ForMember(dest => dest.Permissions, opt => opt.Ignore());

        CreateMap<CreateRoleDto, ApplicationRole>();

        // Permission mappings
        CreateMap<Permission, PermissionDto>();

        // Branch mappings
        CreateMap<CreateBranchDto, Branch>();
        CreateMap<Branch, BranchDto>();

        // Audit log mappings
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Changes, opt => opt.MapFrom(src => src.PropertyChanges));

        CreateMap<AuditLogDetail, AuditLogDetailDto>();
    }
}

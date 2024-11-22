using AutoMapper;
using Falcon.Server.Features.Auth.DTOs;
using Falcon.Server.Features.Auth.Models;

namespace Falcon.Server.Features.Auth
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateEntityToDTOMappings();
            CreateDTOToEntityMappings();
        }

        private void CreateDTOToEntityMappings()
        {
            CreateMap<ApplicationUser, UserDTO>();
        }

        private void CreateEntityToDTOMappings()
        {
            CreateMap<UserDTO, ApplicationUser>();
        }
    }
}
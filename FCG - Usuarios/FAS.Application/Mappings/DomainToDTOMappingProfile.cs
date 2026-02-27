using AutoMapper;
using FAS.API.Models;
using FAS.Application.DTOs;
using FAS.Domain.Entities;

namespace FAS.Application.Mappings
{
    public class EntitiesToDTOMappingProfile : Profile
    {
        public EntitiesToDTOMappingProfile()
        {
            CreateMap<Usuario, UsuarioDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailUsuario.EmailAddress))
                .ReverseMap();
            CreateMap<Usuario, UsuarioViewModel>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailUsuario.EmailAddress))
                .ReverseMap();
        }
    }
}

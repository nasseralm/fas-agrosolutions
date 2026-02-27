using AutoMapper;
using FCG.API.Models;
using FCG.Application.DTOs;
using FCG.Domain.Entities;

namespace FCG.Application.Mappings
{
    public class EntitiesToDTOMappingProfile : Profile
    {
        public EntitiesToDTOMappingProfile()
        {
            CreateMap<Jogo, JogoDTO>().ReverseMap();
            CreateMap<Jogo, JogoViewModel>().ReverseMap();
        }
    }
}

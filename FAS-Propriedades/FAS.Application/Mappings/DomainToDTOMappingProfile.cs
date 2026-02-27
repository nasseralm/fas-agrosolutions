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
            CreateMap<Propriedade, PropriedadeDTO>()
                .ForMember(dest => dest.Localizacao, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Talhao, TalhaoDTO>()
                .ForMember(dest => dest.Delimitacao, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Propriedade, PropriedadeViewModel>()
                .ForMember(dest => dest.Localizacao, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Talhao, TalhaoViewModel>()
                .ForMember(dest => dest.Delimitacao, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}

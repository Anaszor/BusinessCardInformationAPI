using AutoMapper;
using BusinessCardInformationAPI.Domain.Entities;
using BusinessCardInformationAPI.Application.DTOs;

namespace BusinessCardInformationAPI.Application.Mappings
{
    public class BusinessCardProfile : Profile
    {
        public BusinessCardProfile()
        {
            CreateMap<BusinessCard, BusinessCardDto>().ReverseMap();
            CreateMap<BusinessCard, CreateBusinessCardDto>().ReverseMap();
        }
    }
}

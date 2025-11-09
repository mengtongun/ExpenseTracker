using AutoMapper;
using ExpenseTracker.Application.Contracts.Categories;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Mapping;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PublicId));
    }
}


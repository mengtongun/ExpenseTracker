using AutoMapper;
using ExpenseTracker.Application.Contracts.Expenses;
using ExpenseTracker.Domain.Entities;
using System;

namespace ExpenseTracker.Application.Mapping;

public class ExpenseProfile : Profile
{
    public ExpenseProfile()
    {
        CreateMap<Expense, ExpenseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PublicId))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Category != null ? src.Category.PublicId : (Guid?)null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));
    }
}


using AutoMapper;
using ExpenseTracker.Application.Contracts.RecurringExpenses;
using ExpenseTracker.Domain.Entities;
using System;

namespace ExpenseTracker.Application.Mapping;

public class RecurringExpenseProfile : Profile
{
    public RecurringExpenseProfile()
    {
        CreateMap<RecurringExpense, RecurringExpenseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PublicId))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Category != null ? src.Category.PublicId : (Guid?)null));
    }
}


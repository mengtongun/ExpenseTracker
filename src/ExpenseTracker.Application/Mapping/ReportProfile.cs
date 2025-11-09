using AutoMapper;
using ExpenseTracker.Application.Contracts.Reports;
using ExpenseTracker.Domain.Entities;
using System;

namespace ExpenseTracker.Application.Mapping;

public class ReportProfile : Profile
{
    public ReportProfile()
    {
        CreateMap<Report, ReportDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PublicId));
    }
}


using AutoMapper;
using ExpenseTracker.Application.Contracts.Users;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PublicId));
    }
}


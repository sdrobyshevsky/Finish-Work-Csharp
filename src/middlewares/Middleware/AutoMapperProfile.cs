using AutoMapper;
using Middleware.Entity;
using System;
using System.Collections.Generic;

namespace Middleware
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() 
        {
            CreateMap<UserEntity, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Salt, opt => opt.MapFrom(src => src.Salt))
                .ForMember(dest => dest.IsAdmin, opt => opt.MapFrom(src => src.RoleId == 1))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => new List<string> { src.Role.Name }));

            CreateMap<User, UserEntity>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Salt, opt => opt.MapFrom(src => src.Salt))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.IsAdmin ? 1 : 2));

            CreateMap<MessageEntity, GetMessage>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Text))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead))
                .ForMember(dest => dest.FromUser, opt => opt.MapFrom(src => src.FromUserId))
                .ForMember(dest => dest.ToUser, opt => opt.MapFrom(src => src.ToUserId))
                .ForMember(dest => dest.SendDate, opt => opt.MapFrom(src => src.SendDate));

            CreateMap<GetMessage, MessageEntity>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Text))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead))
                .ForMember(dest => dest.FromUserId, opt => opt.MapFrom(src => src.FromUser))
                .ForMember(dest => dest.FromUser, opt => opt.Ignore())
                .ForMember(dest => dest.ToUserId, opt => opt.MapFrom(src => src.ToUser))
                .ForMember(dest => dest.ToUser, opt => opt.Ignore())
                .ForMember(dest => dest.SendDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        }
    }
}

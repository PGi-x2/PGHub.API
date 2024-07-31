﻿using AutoMapper;
using PGHub.API.DTOs.Attachment;
using PGHub.API.DTOs.Post;
using PGHub.Domain.Entities;

namespace PGHub.API.Profiles
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            // Used for GetById
            CreateMap<Post, PostDTO>()
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));
            CreateMap<PostDTO, Post>()
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

            // Map between Attachment and AttachmentDTO
            CreateMap<Attachment, AttachmentDTO>();
            CreateMap<AttachmentDTO, Attachment>();

            // Used for CreatePost
            CreateMap<CreatePostDTO, Post>();
            CreateMap<Post, CreatePostDTO>();

            // Used for UpdatePost
            CreateMap<UpdatePostDTO, Post>();
            CreateMap<Post, UpdatePostDTO>();

        }
    }
}

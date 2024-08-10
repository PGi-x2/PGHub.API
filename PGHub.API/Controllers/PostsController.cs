﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PGHub.Application.DTOs.Post;
using PGHub.DataPersistance;
using PGHub.DataPersistance.Repositories;
using PGHub.Domain.Entities;

namespace PGHub.Common.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IPostsRepository _postsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PostsRepository> _logger;

        public PostsController(DataContext dataContext, IPostsRepository postsRepository, IMapper mapper, ILogger<PostsRepository> logger)
        {
            _dataContext = dataContext;
            _postsRepository = postsRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var post = _dataContext.Posts.Find(id);

            // TODO:To add validators to check if the post(guid) exists in the DB
            if (post == null)
            {
                return NotFound();
            }

            // This maps the properties of the post object to a new instance of the PostDTO class.
            // Maps the properties from domain entity to the DTO object that can be displayed to the client
            var postDTO = _mapper.Map<PostDTO>(post);

            return Ok(postDTO);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var posts = _postsRepository.GetAll();

            var postDTO = _mapper.Map<IEnumerable<PostDTO>>(posts);

            return Ok(postDTO);
        }


        [HttpPost]
        public IActionResult CreatePost(CreatePostDTO createPostDTO)
        {
            // Maps the properties from the DTO object to the domain entity
            var post = _mapper.Map<Post>(createPostDTO);

            //foreach (var attachmentDTO in createPostDTO.Attachments)
            //{
            //    post.Attachments.Add(new Attachment
            //    {
            //        FileName = attachmentDTO.FileName,
            //        //Id = attachmentDTO.Id,
            //        //Id = Guid.NewGuid(), // This should be generated by the database 
            //    });
            //}

            // Creates the post in the database / repository
            var createdPost = _postsRepository.Create(post);

            // TODO: Need to return this via the GetById Method that will have the mapping from domain entity to DTO
            // Maps the properties from the domain entity back to the DTO object to return it in the response
            var postDTO = _mapper.Map<PostDTO>(createdPost);

            return CreatedAtAction(nameof(GetById), new { id = createdPost.Id }, postDTO);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePost(Guid id, UpdatePostDTO updatePostDTO)
        {
            // Retrieve the existing post from the repository
            var existingPost = _postsRepository.Find(id);
            if (existingPost == null)
            {
                return NotFound();
            }

            _mapper.Map(updatePostDTO, existingPost);

            // Clear the existing attachments and add the new ones
            existingPost.Attachments.Clear();
            foreach (var attachmentDTO in updatePostDTO.Attachments)
            {
                existingPost.Attachments.Add(new Attachment
                {
                    FileName = attachmentDTO.FileName,
                    //Id = attachmentDTO.Id,
                });
            }

            // Update the post in the DB via the repository
            var updatedPost = _postsRepository.Update(existingPost);

            // Map back from Post entity to UpdatePostDto to return it in the response / client
            var postDTO = _mapper.Map<UpdatePostDTO>(updatedPost);

            return Ok(postDTO);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePost(Guid id)
        {
            bool isDeleted;

            try
            {
                isDeleted = _postsRepository.Delete(id);

                if (!isDeleted)
                {
                    return NotFound();
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while deleting the post with the ID: {PostId}", id + ".");

                return StatusCode(500, "An error occured while deleting the post.");
            }
        }

    }
}

using Microsoft.EntityFrameworkCore;
using PGHub.Domain.Entities;

namespace PGHub.DataPersistance.Repositories
{
    public class PostsRepository : IPostsRepository
    {
        private readonly DataContext _dataContext;

        public PostsRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        /// <summary>Gets a post by its ID, asynchronously.</summary>
        /// <param name="id">The ID of the post.</param>
        /// <returns>An <see cref="Task{Post?}"/> async of which result contains the post with the specified ID.</returns>
        public async Task<Post?> GetByIdAsync(Guid id)
        {
            return await _dataContext.Posts.Include(p => p.Attachments).FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>Gets all posts, asynchronously.</summary>
        /// <param name="pageNumber">The page number of the posts to be retrieved.</param>
        /// <param name="pageSize">The number of posts to be retrieved per page.</param>
        /// <returns>An <see cref="Task{IEnumerable{Post}}"/> async of which result contains all posts.</returns>
        public async Task<List<Post>> GetAllAsync(int pageNumber, int pageSize)
        {
            // think for big results in order to retrieve first 100 results and then the rest, to be performant
            return await _dataContext.Posts
                .Include(p => p.Attachments) // Includes related Attachments for each Post
                .Skip((pageNumber - 1) * pageSize) // Skips the records of previous pages
                .Take(pageSize) // Takes the number of records specified by pageSize
                .ToListAsync(); // Executes the query asynchronously and returns the results as a list
        }
        /// <summary>Creates a new post, asynchronously</summary>
        /// <param name="post">Represents the post object that contains the necessary properties for creating the post.</param>
        /// <returns>An <see cref="Task{Post}"/> that contains the result of the created post.</returns>
        public async Task<Post> CreateAsync(Post post)
        {
            await using var transaction = await _dataContext.Database.BeginTransactionAsync();
            try
            {
                await _dataContext.Posts.AddAsync(post);
                await _dataContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return post;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("An error occured while creating the post.", ex);
            }

        }

        /// <summary>Updates an existing post, asynchronously.</summary>
        /// <param name="post">The post object that contains all the mapped informations about the post.</param>
        /// <returns>An <see cref="Task{Post}"/> that contains the result of the updated post.</returns>
        public async Task<Post?> UpdateAsync(Guid id, Post post)
        {
            await using var transaction = await _dataContext.Database.BeginTransactionAsync();

            try
            {
                var postDb = _dataContext.Posts.Find(id);

                if (postDb != null)
                {
                    // postDb.Attachments.Clear();
                    post.Id = id;

                    // Clear the existing attachments and add the new ones
                    post.Attachments.Clear();
                    foreach (var attachmentDTO in post.Attachments)
                    {
                        post.Attachments.Add(new Attachment
                        {
                            FileName = attachmentDTO.FileName,
                            //Id = attachmentDTO.Id,
                        });
                    }
                    _dataContext.Entry(postDb).CurrentValues.SetValues(post);
                    await _dataContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return post;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("An error occurred while updating the post.", ex);
            }
        }

        /// <summary>Deletes a post by its ID, asynchronously.</summary>
        /// <param name="id">The ID of the post to delete.</param>
        /// <returns>A <see cref="Task{bool}"/> that indicates whether the post was deleted or not.</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            await using var transaction = await _dataContext.Database.BeginTransactionAsync();
            try
            {
                var post = await _dataContext.Posts.Where(p => p.Id == id).Include(p => p.Attachments).FirstOrDefaultAsync();

                if (post != null)
                {
                    _dataContext.Posts.Remove(post);
                    await _dataContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("An error occurred while deleting the post.", ex);
            }

        }
    }
}

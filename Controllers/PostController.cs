using System.Net;
using api.Data;
using api.DTOs;
using api.Models;
using api.Services.Functions;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;

        public PostController(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            var posts = await _context.Posts
                .Include(c => c.User)
                .Include(i => i.ImagePosts)
                .AsNoTracking()
                .ToListAsync();
            return Ok(posts);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetPostWithComments(string postId)
        {
            var postbyid = await _context.Comments
                .Include(c => c.User)
                .Include(p => p.Post)
                .Include(c => c.ParentComment)
                .Where(p => p.PostId == postId)
                .AsNoTracking()
                .ToListAsync();
            return Ok(postbyid);
        }

        [HttpPost]
        public async Task<IActionResult> AddImagePost([FromForm] ImagePostDto dto)
        {
            string? imgPath = null;
            if (dto.File != null)
            {
                var uploadResult = await _imageService.AddImageAsync(dto.File);
                if (uploadResult.StatusCode == HttpStatusCode.OK)
                    imgPath = uploadResult.SecureUrl.AbsoluteUri;
                else
                    return StatusCode((int)uploadResult.StatusCode, "Image upload failed.");
            }

            var newImagePost = new ImagePost
            {
                ImgId = Guid.NewGuid().ToString(),
                PostId = dto.PostId,
                ImageUrl = imgPath,
            };
            _context.ImagePosts.Add(newImagePost);
            return await _context.SaveChangesAsync() > 0
                ? StatusCode(StatusCodes.Status200OK, "Success")
                : StatusCode(StatusCodes.Status500InternalServerError, "Error");
        }
        
        

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] PostDto dto)
        {
            var post = new Posts
            {
                PostId = Guid.NewGuid().ToString(),
                UserId = dto.UserId,
                Content = dto.Content,
                Status = StatusPost.Pending,
                CreatedAt = DateTime.Now
            };
            _context.Posts.Add(post);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(new { postId = post.PostId }); 
            }

            return StatusCode(StatusCodes.Status500InternalServerError, "Error");
        }
        [HttpPost]
        public async Task<IActionResult> AddComment( [FromForm] CommentDto dto)
        {
            var post = await _context.Posts.FindAsync(dto.PostId);
            if (post == null)
                return NotFound("Post not found");

            if (!string.IsNullOrEmpty(dto.ParentCommentId))
            {
                var parentExists = await _context.Comments.AnyAsync(c => c.Id == dto.ParentCommentId);
                if (!parentExists)
                    return BadRequest("Parent comment does not exist");
            }
            string? imgPath = null;
            if (dto.File != null)
            {
                var uploadResult = await _imageService.AddImageAsync(dto.File);
                if (uploadResult.StatusCode == HttpStatusCode.OK)
                    imgPath = uploadResult.SecureUrl.AbsoluteUri;
                else
                    return StatusCode((int)uploadResult.StatusCode, "Image upload failed.");
            }

            var comment = new Comments
            {
                Id = Guid.NewGuid().ToString(),
                PostId = dto.PostId,
                UserId = dto.UserId,
                Content = dto.Content,
                ParentCommentId = string.IsNullOrEmpty(dto.ParentCommentId) ? null : dto.ParentCommentId,
                CreatedAt = DateTime.Now,
                Image = imgPath ?? string.Empty
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);
        }
        
        [HttpDelete]
        public async Task<IActionResult> DeletePost(string postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .ToListAsync();
            if (comments.Any())
                _context.Comments.RemoveRange(comments);

            var imagePosts = await _context.ImagePosts
                .Where(ip => ip.PostId == postId)
                .ToListAsync();
            if (imagePosts.Any())
                _context.ImagePosts.RemoveRange(imagePosts);

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId);
            if (post == null)
                return NotFound("Post not found.");

            _context.Posts.Remove(post);

            var result = await _context.SaveChangesAsync();
            return result > 0
                ? Ok("Success")
                : StatusCode(StatusCodes.Status500InternalServerError, "Error");
        }
    
        [HttpDelete]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound("Comment not found.");
            _context.Comments.Remove(comment);
            return await _context.SaveChangesAsync() > 0
                ? StatusCode(StatusCodes.Status200OK, "Success")
                : StatusCode(StatusCodes.Status500InternalServerError, "Error");
        }
        
    }

   
}

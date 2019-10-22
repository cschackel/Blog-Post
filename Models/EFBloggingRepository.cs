using System;
using System.Linq;

namespace Blogs.Models
{
    public class EFBloggingRepository : IBloggingRepository
    {
        // the repository class depends on the BloggingContext service
        // which was registered at application startup
        private BloggingContext context;
        public EFBloggingRepository(BloggingContext ctx)
        {
            context = ctx;
        }
        // create IQueryable for Blogs & Posts
        public IQueryable<Blog> Blogs => context.Blogs;
        public IQueryable<Post> Posts => context.Posts;

        public void AddBlog(Blog blog)
        {
            context.Add(blog);
            context.SaveChanges();
        }

        public void DeleteBlog(Blog blog)
        {
            context.Remove(blog);
            context.SaveChanges();
        }

        public void AddPost(Post post)
        {
            context.Add(post);
            context.SaveChanges();
        }

        public void DeletePost(Post post)
        {
            context.Remove(post);
            context.SaveChanges();
        }
    }
}

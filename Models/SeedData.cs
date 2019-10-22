using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Blogs.Models
{
    // static class cannot be instantiated
    public static class SeedData
    {
        public static void EnsurePopulated(IApplicationBuilder app)
        {
            // since we are using this scoped service in the configure method,
            // we must disable the validate scopes option in Program.cs
            // as soon as the database is seeded, we should remove the call
            // to this class in Startup.cs and enable scope validation in Program.cs 
            BloggingContext context = app.ApplicationServices.GetRequiredService<BloggingContext>();
            if (!context.Blogs.Any())
            {
                context.Blogs.Add(new Blog { Name = "Tortoise" });
                context.Blogs.Add(new Blog { Name = "Hare" });
                context.SaveChanges();
            }
        }
    }
}
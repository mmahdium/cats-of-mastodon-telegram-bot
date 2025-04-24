using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using mstdnCats.Models;

namespace mstdnCats.Services;

public class ServerStartup
{
    private static IMongoCollection<Post> _db;
    public static void Serverstartup(IMongoCollection<Post> db)
    {
        _db = db;
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseCors();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context =>
            {
                var assembly = Assembly.GetEntryAssembly();
                var resourceName = "mstdnCats.Web.wwwroot.index.html"; // Full resource name

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Something went wrong in our side.");
                        return;
                    }

                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        context.Response.ContentType = "text/html";
                        await context.Response.WriteAsync(reader.ReadToEnd());
                    }
                }
            });

            endpoints.MapGet("/api/gimme", async context =>
            {
                // Api endpoint
                // Measure execution time
                var stopwatch = Stopwatch.StartNew();

                // Choose all posts media attachments that are approved
                var filter = Builders<Post>.Filter.ElemMatch(post => post.MediaAttachments,
                    Builders<MediaAttachment>.Filter.Eq(media => media.Approved, true));
                var projection = Builders<Post>.Projection
                    .Include(p => p.Url)
                    .Include(p => p.Account.DisplayName)
                    .Include(p => p.MediaAttachments);
                
                var selectedPost = await _db.Aggregate().Match(filter).Project<Post>(projection).Sample(1).FirstOrDefaultAsync();

                // Stop and print execution time
                stopwatch.Stop();
                Console.WriteLine($"Query executed in: {stopwatch.ElapsedMilliseconds} ms");
                
                // Send as JSON
                selectedPost.MediaAttachments = selectedPost.MediaAttachments
                    .Where(media => media.Approved)
                    .ToList();
                await context.Response.WriteAsJsonAsync(selectedPost);
            });
        });
    }
}
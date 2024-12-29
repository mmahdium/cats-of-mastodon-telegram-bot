using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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
                // Choose all media attachments that are approved
                var mediaAttachmentsToSelect = await _db.AsQueryable()
                    .Where(post => post.MediaAttachments.Any(media => media.Approved))
                    .ToListAsync();

                // Select random approved media attachment
                var selectedPost = mediaAttachmentsToSelect[new Random().Next(mediaAttachmentsToSelect.Count)];
                
                // Send as JSON
                await context.Response.WriteAsJsonAsync(selectedPost);
            });
        });
    }
}
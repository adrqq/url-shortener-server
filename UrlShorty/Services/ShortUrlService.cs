using Microsoft.EntityFrameworkCore;
using NanoidDotNet;
using UrlShorty.Data;
using UrlShorty.Exceptions;
using UrlShorty.Models;

namespace UrlShorty.Services
{
    public class ShortUrlService
    {
        private readonly ApplicationDbContext _context;

        public ShortUrlService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> ValidateAndCreateShortUrlAsync(ShortenUrlRequestModel model)
        {
            model.Slug = string.IsNullOrEmpty(model.Slug) ? Nanoid.Generate(size: 5).ToLower() : model.Slug.ToLower();

            var existingUrlRedir = await _context.Redirection.FirstOrDefaultAsync(u => u.Url == model.Url);
            var existingSlugRedir = await _context.Redirection.FirstOrDefaultAsync(u => u.Slug == model.Slug);

            var newRedirection = new RedirectionModel
            {
                Url = model.Url,
                Slug = model.Slug,
            };

            if (existingUrlRedir == null && existingSlugRedir == null)
            {
                _context.Redirection.Add(newRedirection);

                await _context.SaveChangesAsync();
            }

            var existingUrl = await _context.ShortUrl.FirstOrDefaultAsync(u => u.Url == model.Url);
            var existingSlug = await _context.ShortUrl.FirstOrDefaultAsync(u => u.Slug == model.Slug);

            if (existingUrl != null || existingSlug != null)
            {
                throw new ApiError(400, "Record on this url or slug already exists");
            }

            if (!string.IsNullOrEmpty(model.Nickname))
            {

                var newUrl = new ShortUrlModel
                {
                    Url = model.Url,
                    Slug = model.Slug,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.Nickname
                };

                _context.ShortUrl.Add(newUrl);
                await _context.SaveChangesAsync();

                return newUrl.Slug;
            }

            return newRedirection.Slug;
        }

        public List<ShortUrlModel> GetAllRecords()
        {
            var allRecords = _context.ShortUrl.ToList();

            return allRecords;
        }

        public async Task<string> RedirectionAsync(string slug)
        {
            var url = await _context.ShortUrl.FirstOrDefaultAsync(u => u.Slug == slug);

            if (String.IsNullOrEmpty(url?.Url))
            {
                var urlRedirect = await _context.Redirection.FirstOrDefaultAsync(u => u.Slug == slug) ?? throw new ApiError(400, "No such short url");

                return urlRedirect.Url;
            }

            return url.Url;
        }

        public async Task<string> FetchAboutAsync()
        {
            var model = await _context.About.FirstOrDefaultAsync() ?? throw new ApiError(400, "Bad request");

            return model.Text;
        }

        public async Task<string> UpdateAboutAsync(string newText)
        {
            var aboutModel = await _context.About.FirstOrDefaultAsync();

            if (aboutModel == null)
            {
                aboutModel = new AboutTextModel
                {
                    Text = newText
                };

                await _context.About.AddAsync(aboutModel);

                await _context.SaveChangesAsync();
            }
            else
            {
                aboutModel.Text = newText;

                await _context.SaveChangesAsync();
            }

            return aboutModel.Id != Guid.Empty ? "Updated" : "First created";
        }
    }
}

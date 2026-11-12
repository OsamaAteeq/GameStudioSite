using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;
namespace GameStudioSite.Services
{
    public class ImageUploadService
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
        private const long MaxPixels = 25_000_000;
        private const int JpegQuality = 80;
        private static readonly int[] VariantWidths = { 640, 1280, 1920 };
        private const int VariantQuality = 78;
        private static readonly ConcurrentDictionary<string, int> WidthCache = new();

        private static bool IsBlogJpeg(string url) =>
            url.StartsWith("/images/blog/", StringComparison.OrdinalIgnoreCase)
            && url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase);

        private static bool IsVariantName(string nameWithoutExtension) =>
            VariantWidths.Any(w => nameWithoutExtension.EndsWith($"-{w}", StringComparison.OrdinalIgnoreCase));

        private static string VariantUrl(string imageUrl, int width) =>
            $"{imageUrl[..^4]}-{width}.jpg";

        private string FullPath(string webPath) =>
            Path.Combine(_env.WebRootPath, webPath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));

        private void DeleteFile(string webPath)
        {
            var fullPath = FullPath(webPath);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }

        private int GetWidth(string imageUrl) =>
            WidthCache.GetOrAdd(imageUrl, url =>
            {
                try { return Image.Identify(FullPath(url))?.Width ?? 0; }   // reads the header only
                catch { return 0; }
            });

        private static async Task SaveVariantsAsync(Image image, string folder, string baseName)
        {
            foreach (var w in VariantWidths)
            {
                if (image.Width <= w) continue;
                var path = Path.Combine(folder, $"{baseName}-{w}.jpg");
                if (File.Exists(path)) continue;
                using var copy = image.Clone(x => x.Resize(w, 0));   // 0 = keep proportions
                await copy.SaveAsJpegAsync(path, new JpegEncoder { Quality = VariantQuality });
            }
        }

        // Returns a srcset value, or null (the attribute is then left out and src is used alone).
        public string? GetSrcSet(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || !IsBlogJpeg(imageUrl)) return null;

            var parts = new List<string>();
            foreach (var w in VariantWidths)
            {
                var url = VariantUrl(imageUrl, w);
                if (File.Exists(FullPath(url))) parts.Add($"{url} {w}w");
            }
            if (parts.Count == 0) return null;

            var originalWidth = GetWidth(imageUrl);
            if (originalWidth > 0) parts.Add($"{imageUrl} {originalWidth}w");
            return string.Join(", ", parts);
        }

        public async Task BackfillBlogVariantsAsync()
        {
            var folder = Path.Combine(_env.WebRootPath, "images", "blog");
            if (!Directory.Exists(folder)) return;

            foreach (var path in Directory.EnumerateFiles(folder, "*.jpg"))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (IsVariantName(name)) continue;
                try
                {
                    var info = await Image.IdentifyAsync(path);
                    if (info == null) continue;
                    var missing = VariantWidths.Any(w => info.Width > w && !File.Exists(Path.Combine(folder, $"{name}-{w}.jpg")));
                    if (!missing) continue;

                    using var image = await Image.LoadAsync(path);
                    await SaveVariantsAsync(image, folder, name);
                }
                catch (Exception) { /* skip unreadable files */ }
            }
        }
        // Longest side in pixels per upload folder. Smaller images are never enlarged.
        private static readonly Dictionary<string, int> MaxSide = new(StringComparer.OrdinalIgnoreCase)
        {
            ["blog"] = 2560,
            ["games"] = 2000,
        };
        private const int DefaultMaxSide = 2000;

        public ImageUploadService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var normalized = "/" + imageUrl.TrimStart('~', '/');   // handles both "~/images/..." and "/images/..."
            DeleteFile(normalized);
            if (IsBlogJpeg(normalized))
            {
                foreach (var w in VariantWidths) DeleteFile(VariantUrl(normalized, w));
                WidthCache.TryRemove(normalized, out _);
            }
        }

        public async Task<string?> SaveImageAsync(IFormFile? file, string subfolder)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only .jpg, .jpeg, .png, and .webp files are allowed.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                throw new InvalidOperationException("Image must be smaller than 5MB.");
            }

            using var input = new MemoryStream();
            await file.CopyToAsync(input);

            Image image;
            try
            {
                input.Position = 0;
                var info = await Image.IdentifyAsync(input);
                if (info == null)
                    throw new InvalidOperationException("The file is not a valid image.");
                if ((long)info.Width * info.Height > MaxPixels)
                    throw new InvalidOperationException("The image has too many pixels. Please use a smaller one.");

                input.Position = 0;
                image = await Image.LoadAsync(input);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception)   // unknown format or corrupt data
            {
                throw new InvalidOperationException("The file is not a valid image.");
            }

            using (image)
            {
                image.Mutate(x => x.AutoOrient());   // apply the camera's rotation first

                var maxSide = MaxSide.TryGetValue(subfolder, out var m) ? m : DefaultMaxSide;
                if (Math.Max(image.Width, image.Height) > maxSide)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(maxSide, maxSide),
                        Mode = ResizeMode.Max   // fit inside the box, keep proportions
                    }));
                }

                image.Mutate(x => x.BackgroundColor(Color.White));  // fill any transparency

                // Remove camera and location data (keep the colour profile)
                image.Metadata.ExifProfile = null;
                image.Metadata.XmpProfile = null;
                image.Metadata.IptcProfile = null;

                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", subfolder);
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}.jpg";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = JpegQuality });
                if (subfolder.Equals("blog", StringComparison.OrdinalIgnoreCase))
                    await SaveVariantsAsync(image, uploadsFolder, Path.GetFileNameWithoutExtension(uniqueFileName));
                return $"/images/{subfolder}/{uniqueFileName}";
            }
        }
    }
}
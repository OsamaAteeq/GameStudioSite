using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
namespace GameStudioSite.Services
{
    public class ImageUploadService
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
        private const long MaxPixels = 25_000_000;
        private const int JpegQuality = 80;

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
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            // imageUrl looks like "/images/games/xxxx.jpg" — convert that web path to a real file path
            var relativePath = imageUrl.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
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

                return $"/images/{subfolder}/{uniqueFileName}";
            }
        }
    }
}
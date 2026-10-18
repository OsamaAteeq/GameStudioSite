namespace GameStudioSite.Services
{
    public class ImageUploadService
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

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
            var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
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

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", subfolder);
            Directory.CreateDirectory(uploadsFolder); // no-op if it already exists

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{subfolder}/{uniqueFileName}";
        }
    }
}
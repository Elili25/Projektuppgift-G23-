namespace SeventWebApplication.Models.Admin
{
    // Tar emot bilder som personal laddar upp för fordon och fordonsfamiljer.
    // Filerna sparas under wwwroot/uploads/<mapp>/ med ett slumpat filnamn (aldrig användarens eget),
    // och det som sparas på fordonet/familjen är webbadressen, t.ex. /uploads/vehicles/3f2a....png.
    public class ImageUploadService
    {
        private const long MaxBytes = 5 * 1024 * 1024;   // 5 MB

        private readonly string _webRoot;

        public ImageUploadService(IWebHostEnvironment env)
        {
            _webRoot = env.WebRootPath;
        }

        // Kollar filen utan att spara den. Kastar ArgumentException med ett läsbart meddelande om den inte duger.
        // Typen avgörs av filens innehåll (första byten), inte av filnamnet eller vad webbläsaren påstår.
        public void Validate(IFormFile file)
        {
            if (file.Length == 0)
                throw new ArgumentException("The image file is empty.");

            if (file.Length > MaxBytes)
                throw new ArgumentException("The image is too large (max 5 MB).");

            if (DetectExtension(file) is null)
                throw new ArgumentException("The image must be a PNG, JPG or WebP file.");
        }

        // Sparar bilden och returnerar webbadressen till den. folder = "vehicles" eller "families".
        public async Task<string> SaveAsync(IFormFile file, string folder)
        {
            Validate(file);
            string extension = DetectExtension(file)!;

            string directory = Path.Combine(_webRoot, "uploads", folder);
            Directory.CreateDirectory(directory);

            string fileName = $"{Guid.NewGuid():N}{extension}";
            await using var stream = File.Create(Path.Combine(directory, fileName));
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }

        // Känner igen PNG, JPEG och WebP på filens första byte
        private static string? DetectExtension(IFormFile file)
        {
            var header = new byte[12];
            using var stream = file.OpenReadStream();
            int read = stream.Read(header, 0, header.Length);
            if (read < 12) return null;

            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                return ".png";

            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return ".jpg";

            // WebP: "RIFF" .... "WEBP"
            if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
                return ".webp";

            return null;
        }
    }
}

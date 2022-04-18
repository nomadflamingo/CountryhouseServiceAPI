using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CountryhouseService.API.Data;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;

namespace CountryhouseService.API.Repositories
{
    public class ImagesRepository<TImage> : IImagesRepository<TImage> where TImage : Image
    {
        private readonly Cloudinary _cloudinary;
        private readonly AppDbContext _db;

        public ImagesRepository(AppDbContext db, Cloudinary cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }


        public async Task<TImage?> GetAsync(int id)
        {
            TImage? image = await _db.Set<TImage>().FindAsync(id);
            return image;
        }

        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<Uri> UploadToServerAsync(string base64, string? name)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(base64),
                PublicId = name
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            var code = (int)uploadResult.StatusCode;
            if (code >= 200 && code < 300) return uploadResult.SecureUrl;
            else throw new HttpRequestException($"Cannot upload image. Cloudinary service responded with status code {code}");
        }


        public async Task<Uri> DeleteFromServerAsync(Uri url)
        {
            string publicId = Path.GetFileName(url.LocalPath);
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);
            var code = (int)result.StatusCode;

            if (code >= 200 && code < 300) return url;
            else throw new HttpRequestException($"Cannot destroy image. Cloudinary service responded with status code {code}");
        }


        public async Task<int> AddToDbAsync(TImage image)
        {
            await _db.Set<TImage>().AddAsync(image);
            return image.Id;
        }


        public void Update(TImage image)
        {
            _db.Set<TImage>().Update(image);
        }


        public void Remove(TImage image)
        {
            _db.Set<TImage>().Remove(image);
        }
    }
}

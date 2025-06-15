using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Configuration;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");

            var uploadResult = new ImageUploadResult();

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "flower-shop/products",
                    Transformation = new Transformation()
                        .Width(800)
                        .Height(600)
                        .Crop("fill")
                        .Quality("auto")
                };

                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            if (uploadResult.Error != null)
                throw new Exception($"Image upload failed: {uploadResult.Error.Message}");

            return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
        }

        public async Task<List<(string Url, string PublicId)>> UploadMultipleImagesAsync(List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return new List<(string Url, string PublicId)>();

            var uploadTasks = files.Select(UploadImageAsync);
            var results = await Task.WhenAll(uploadTasks);
            return results.ToList();
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
    }
}
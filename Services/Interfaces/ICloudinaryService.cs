using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file);
        Task<List<(string Url, string PublicId)>> UploadMultipleImagesAsync(List<IFormFile> files);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
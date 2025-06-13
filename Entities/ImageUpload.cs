using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class ImageUpload
    {
        public long Id { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public long FileSize { get; set; }
        public bool isFeaturedImage { get; set; }

        public long? ProductId { get; set; }
        public Product Product { get; set; }

        public long? UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ImageType ImageType { get; set; }
    }
}
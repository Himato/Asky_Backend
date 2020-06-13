using System;
using System.IO;
using System.Linq;
using Asky.Helpers;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Asky.Services
{
    public class ImageService
    {
        public static void DeleteImage(string uri)
        {
            File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", uri));
        }
        
        public static string SaveImage(IFormFile file)
        {
            var size = file.Length / 1024f / 1024f;

            if (size > 25)
            {
                throw new ArgumentException("Max image size is 25MB");
            }

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                bytes = ms.ToArray();
            }

            if (!FileHelper.IsImage(bytes))
            {
                throw new ArgumentException("Unsupported Image Type");
            }

            try
            {
                var name = Guid.NewGuid() + "." + file.FileName.Split('.').Last();

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", name);

                // Using SixLabors.ImageSharp Package from https://github.com/SixLabors/ImageSharp
                using (var image = Image.Load(file.OpenReadStream()))
                {
                    var ratio = (int) Math.Ceiling(size);
                    var width = image.Width / ratio;
                    var height = image.Height / ratio;

                    image.Mutate(c => c.Resize(width, height));
                    image.Save(path);
                }

                //using (var bits = new FileStream(path, FileMode.Create))
                //{
                //    await image.CopyToAsync(bits);
                //}

                return name;
            }
            catch
            {
                return null;
            }
        }
    }
}

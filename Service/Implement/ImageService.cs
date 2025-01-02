using DataAccess;
using DataAccess.Models;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement {
    public class ImageService : IImageService {
        private readonly IConfiguration _config;
        private readonly IProductImageDAO _productImageDAO;

        public ImageService(IConfiguration config, IProductImageDAO productImageDAO) {
            _config = config;
            _productImageDAO = productImageDAO;
        }

        public async Task<string> StoreImageAsync(string fileName, Stream stream) {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(_config["Firebase:ApiKey"]));
            var a = await auth.SignInWithEmailAndPasswordAsync(_config["Firebase:AuthEmail"], _config["Firebase:AuthPassword"]);

            // Constructr FirebaseStorage, path to where you want to upload the file and Put it there
            var task = new FirebaseStorage(
                _config["Firebase:Bucket"],

                 new FirebaseStorageOptions {
                     AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                     ThrowOnCancel = true,
                 })
                .Child("img")
                .Child("avt")
                .Child(fileName)
                .PutAsync(stream);

            // Track progress of the upload
            //task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

            // await the task to wait until upload completes and get the download url
            var downloadUrl = await task;
            return downloadUrl;
        }

        public ProductImage GetMainImageByProductId(int productId)
        {
            return _productImageDAO.GetByProductId(productId).OrderBy(i => i.CreatedAt).FirstOrDefault();
        }

        public List<ProductImage> GetAllImagesByProductId(int productId)
        {
            return _productImageDAO.GetByProductId(productId).OrderBy(i => i.CreatedAt).ToList();
        }

        public void SaveProductImage(int productId, string imageUrl)
        {
            ProductImage productImage = new ProductImage()
            {
                Id = 0,
                ProductId = productId,
                Status = (int)Status.Available,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _productImageDAO.Create(productImage);
        }
    }
}

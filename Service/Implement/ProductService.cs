using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Service.Implement
{
    public class ProductService : IProductService
    {
        private readonly IProductDAO _productDAO;
        private readonly IAuctionDAO _auctionDAO;
        private readonly IProductImageDAO _productImageDAO;

        public ProductService(IProductDAO productDAO, IAuctionDAO auctionDAO, IProductImageDAO productImageDAO)
        {
            _productDAO = productDAO;
            _auctionDAO = auctionDAO;
            _productImageDAO = productImageDAO;
        }

        public List<Product> GetProducts(string? nameSearch, int materialId, int categoryId, int type, decimal priceMin, decimal priceMax, int orderBy)
        {
            List<Product> productList;
            try
            {
                var products = _productDAO.GetProducts()
                    .Where(p => p.Status == (int)Status.Available
                    && p.Seller.Status == (int)SellerStatus.Available
                    && (string.IsNullOrEmpty(nameSearch) || p.Name.Contains(nameSearch))
                    && (materialId == 0 || p.MaterialId == materialId)
                    && (categoryId == 0 || p.CategoryId == categoryId)
                    && (type == 0 || p.Type == type)
                    //&& (condition == 0 || p.Condition == condition)
                    //&& (ratings == 0 || p.Ratings == ratings)
                    && p.Price >= priceMin
                    && (priceMax == 0 || p.Price <= priceMax));

                //default (0): new -> old , 1: old -> new, 2: low -> high, 3: high -> low
                switch (orderBy)
                {
                    case 1:
                        products = products.OrderBy(p => p.CreatedAt);
                        break;
                    case 2:
                        products = products.OrderBy(p => p.Price);
                        break;
                    case 3:
                        products = products.OrderByDescending(p => p.Price);
                        break;
                    default:
                        products = products.OrderByDescending(p => p.CreatedAt);
                        break;
                }

                productList = products
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return productList;
        }

        public Product GetProductById(int id)
        {
            if (id == null)
            {
                throw new Exception("404: Không tìm thấy sản phẩm");
            } 
            return _productDAO.GetProductById(id);
        }

        public List<Product> GetProductsBySellerId(int sellerId)
        {
            if (sellerId == null)
            {
                throw new Exception("404: Không tìm thấy người bán");
            }
            return _productDAO.GetProductsBySellerId(sellerId).ToList();
        }

        public int CreateProduct(Product product, Auction auction)
        {
            product.Status = (int) Status.Available;
            product.Ratings = 0;
            product.CreatedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;
            _productDAO.CreateProduct(product);

            if(product.Type == (int) ProductType.Auction)
            {
                auction.ProductId = product.Id;
                auction.StartingPrice = product.Price;
                if (0 < auction.StartingPrice && auction.StartingPrice <= 20000000)
                {
                    auction.EntryFee = 50000;
                }
                else if (20000000 < auction.StartingPrice && auction.StartingPrice <= 50000000)
                {
                    auction.EntryFee = 100000;
                } 
                else if (50000000 < auction.StartingPrice && auction.StartingPrice <= 100000000)
                {
                    auction.EntryFee = 150000;
                }
                else if(100000000 < auction.StartingPrice && auction.StartingPrice <= 500000000)
                {
                    auction.EntryFee = 200000;
                }
                else 
                {
                    auction.EntryFee = 500000;
                }
                auction.StaffId = null;
                auction.Status = (int) AuctionStatus.Pending;
                auction.CreatedAt = DateTime.Now;
                auction.UpdatedAt = DateTime.Now;
                _auctionDAO.CreateAuction(auction);
            }          
            return product.Id;
        }

        public Product UpdateProduct(int id, Product product, Auction auction)
        {
            if (id == null) throw new Exception("404: Không tìm thấy sản phẩm");

            Product currentProduct = _productDAO.GetProductById(id);

            currentProduct.CategoryId = product.CategoryId;
            currentProduct.MaterialId = product.MaterialId;
            currentProduct.Name = product.Name;
            currentProduct.Description = product.Description;
            currentProduct.Price = product.Price;
            currentProduct.Dimension = product.Dimension;
            currentProduct.Weight = product.Weight;
            currentProduct.Origin = product.Origin;
            currentProduct.PackageMethod = product.PackageMethod;
            currentProduct.PackageContent = product.PackageContent;
            currentProduct.Condition = product.Condition;
            currentProduct.Type = product.Type;
            currentProduct.UpdatedAt = DateTime.Now;

            _productDAO.UpdateProduct(currentProduct);

            if (product.Type == (int) ProductType.Auction && auction.Status == (int) AuctionStatus.Rejected) // bi reject moi cho sua lai
            {
                Auction currentAuction = _auctionDAO.GetAuctionById(id);
                currentAuction.StartingPrice = product.Price;
                currentAuction.EntryFee = 0.1m * auction.StartingPrice;
                currentAuction.Status = (int) AuctionStatus.Pending;
                currentAuction.UpdatedAt = DateTime.Now;
                _auctionDAO.UpdateAuction(auction);
            }

            return currentProduct;
        }

        public Product DeleteProduct(int id, int sellerId)
        {
            if (id == null) throw new Exception("404: Không tìm thấy sản phẩm");

            Product currentProduct = _productDAO.GetProductById(id);
            if(currentProduct.SellerId != sellerId)
            {
                throw new Exception("400: Sản phẩm này không phải là sản phẩm của bạn");
            }

            //if (currentProduct.Type == (int)ProductType.Auction)
            //{
            //    List<Auction> auctionLists = _auctionDAO.GetAuctionsByProductId(id).ToList();

            //    foreach (Auction auction in auctionLists)
            //    {
            //        if (auction.Status != (int)AuctionStatus.Pending)
            //        {
            //            throw new Exception("400: This product has an active auction.");
            //        }
            //        auction.Status = (int)AuctionStatus.Unavailable;
            //        auction.UpdatedAt = DateTime.Now;
            //        _auctionDAO.UpdateAuction(auction);
            //    }
            //}

            currentProduct.Status = (int)Status.Unavailable;
            currentProduct.UpdatedAt = DateTime.Now;
            _productDAO.UpdateProduct(currentProduct);

            return currentProduct;
        }
    }
}

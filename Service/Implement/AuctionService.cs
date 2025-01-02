using DataAccess;
using DataAccess.Implement;
using DataAccess.Models;
using Hangfire;
using Hangfire.Storage;
using MailKit.Search;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Asn1.Ocsp;
using Request;
using Request.ThirdParty.GHN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Request.ThirdParty.GHN.ShippingOrder;

namespace Service.Implement
{
    public class AuctionService : IAuctionService
    {
        private readonly IAuctionDAO _auctionDAO;
        private readonly IBidDAO _bidDAO;
        private readonly IUserDAO _userDAO;
        private readonly IWalletDAO _walletDAO;
        private readonly ITransactionDAO _transactionDAO;
        private readonly IOrderDAO _orderDAO;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IAddressDAO _addressDAO;
        private readonly IWalletService _walletService;
        private readonly IOrderService _orderService;
        private readonly IProductImageDAO _productImageDAO;
        private readonly IConfiguration _config;
        private IHttpClientFactory _httpClientFactory;
        private readonly ISellerDAO _sellerDAO;
        private readonly IProductDAO _productDAO;

        public AuctionService (IAuctionDAO auctionDAO, IBackgroundJobClient backgroundJobClient, IUserDAO userDAO, 
            IBidDAO bidDAO, IWalletDAO walletDAO, ITransactionDAO transactionDAO, IOrderDAO orderDAO, 
            IAddressDAO addressDAO, IWalletService walletService, IProductImageDAO productImageDAO, IOrderService orderService,
            IConfiguration config, IHttpClientFactory httpClientFactory, ISellerDAO sellerDAO, IProductDAO productDAO)
        {
            _auctionDAO = auctionDAO;
            _userDAO = userDAO;
            _bidDAO = bidDAO;
            _backgroundJobClient = backgroundJobClient;
            _addressDAO = addressDAO;
            _orderDAO = orderDAO;
            _walletService = walletService;
            _walletDAO = walletDAO;
            _transactionDAO = transactionDAO;
            _orderService = orderService;
            _productImageDAO = productImageDAO;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _sellerDAO = sellerDAO;
            _productDAO = productDAO;
        }

        public List<Auction> GetAuctions(string? title, int status, int categoryId, int materialId, int orderBy)
        {
            List<Auction> auctionList;
            try
            {
                var auctions = _auctionDAO.GetAuctions()
                    .Where(a => status == -1 || a.Status == status
                    //&& a.Product.SellerId. == (int)Status.Available
                    && (string.IsNullOrEmpty(title) || a.Title.Contains(title))
                    && (materialId == 0 || a.Product.MaterialId == materialId)
                    && (categoryId == 0 || a.Product.CategoryId == categoryId)
                    && (a.RegistrationStart < DateTime.Now && DateTime.Now < a.RegistrationEnd));

                //default (0): old -> new, 1: started at asc, 2: unknown, 3: unknown
                switch (orderBy)
                {
                    case 1:
                        auctions = auctions.OrderByDescending(a => a.StartedAt);
                        break;
                    case 2:
                        auctions = auctions.OrderBy(p => p.StartingPrice);
                        break;
                    case 3:
                        auctions = auctions.OrderByDescending(p => p.StartingPrice);
                        break;
                    default:
                        auctions = auctions.OrderBy(a => a.StartedAt);
                        break;
                }

                auctionList = auctions
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return auctionList;
        }

        public List<Auction> GetAllAuctions(string? title, int status, int categoryId, int materialId, int orderBy)
        {
            List<Auction> auctionList;
            try
            {
                var auctions = _auctionDAO.GetAuctions()
                    .Where(a => status == -1 || a.Status == status
                    //&& a.Product.SellerId. == (int)Status.Available
                    && (string.IsNullOrEmpty(title) || a.Title.Contains(title))
                    && (materialId == 0 || a.Product.MaterialId == materialId)
                    && (categoryId == 0 || a.Product.CategoryId == categoryId));

                //default (0): old -> new, 1: started at asc, 2: unknown, 3: unknown
                switch (orderBy)
                {
                    case 1:
                        auctions = auctions.OrderBy(a => a.CreatedAt);
                        break;
                    case 2:
                        auctions = auctions.OrderBy(p => p.StartingPrice);
                        break;
                    case 3:
                        auctions = auctions.OrderByDescending(p => p.StartingPrice);
                        break;
                    default:
                        auctions = auctions.OrderByDescending(a => a.CreatedAt);
                        break;
                }

                auctionList = auctions
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return auctionList;
        }

        public List<Auction> GetAuctionAssigned(int staffId)
        {
            if (staffId == null)
            {
                throw new Exception("404: Không tìm thấy nhân viên");
            }
            return _auctionDAO.GetAuctionAssigned(staffId).OrderByDescending(a => a.CreatedAt).ToList();
        }

        public List<Auction> GetAuctionsDoneAssignedByMonths(int staffId, int month)
        {
            if (staffId == null)
            {
                throw new Exception("404: Không tìm thấy nhân viên");
            }
            DateTime filterMonth = DateTime.Now.AddMonths(-month);
            return _auctionDAO.GetAuctionAssigned(staffId)
                .Where(a => (a.Status == (int)AuctionStatus.Ended)
                && a.StartedAt >= filterMonth)
                .OrderByDescending(a => a.StartedAt)
                .ToList();
        }

        public List<Auction> GetAuctionsDoneByMonths(int month)
        {
            DateTime filterMonth = DateTime.Now.AddMonths(-month);
            return _auctionDAO.GetAuctions()
                .Where(a => (a.Status == (int)AuctionStatus.Ended)
                && a.StartedAt >= filterMonth)
                .OrderByDescending(a => a.StartedAt)
                .ToList();
        }

        public List<Auction> GetAuctionsByProductId(int productId)
        {
            if (productId == null)
            {
                throw new Exception("404: Không tìm thấy sản phẩm");
            }
            return _auctionDAO.GetAuctionsByProductId(productId).ToList();
        }

        public Auction GetAuctionById(int id)
        {
            if (id == null)
            {
                throw new Exception("404: Không tìm thấy cuộc đấu giá");
            }
            return _auctionDAO.GetAuctionById(id);
        }

        public List<Auction> GetAuctionBySellerId(int sellerId, int status)
        {
            if (sellerId == null)
            {
                throw new Exception("404: Không tìm thấy người bán");
            }
            return _auctionDAO.GetAuctionBySellerId(sellerId)
                .Where(a => status == -1 || a.Status == status)
                .ToList();
        }

        public List<Auction> GetAuctionJoined(int bidderId)
        {
            if (bidderId == null)
            {
                throw new Exception("404: Không tìm thấy người tham gia");
            }
            return _auctionDAO.GetAuctionJoined(bidderId).ToList();
        }

        public List<Auction> GetAuctionJoinedByStatus(int status, int bidderId)
        {
            if (bidderId == null)
            {
                throw new Exception("404: Không tìm thấy người tham gia");
            }
            return _auctionDAO.GetAuctionJoined(bidderId).Where(a => status == -1 || a.Status == status).ToList();
        }

        public void CreateAuction(Auction auction)
        {
            auction.EntryFee = 0.05m * auction.StartingPrice;
            auction.StaffId = null;
            auction.Status = (int) AuctionStatus.Unassigned;
            auction.CreatedAt = DateTime.Now;
            auction.UpdatedAt = DateTime.Now;
            _auctionDAO.CreateAuction(auction);
        }

        public void AssignStaff(int id, int staffId)
        {
            if (id == null)
            {
                throw new Exception("404: Không tìm thấy cuộc đấu giá");
            } 
            else if (staffId == null || _userDAO.Get(staffId).Role != (int)Role.Staff)
            {
                throw new Exception("404: Không tìm thấy nhân viên");
            }
            Auction auction = _auctionDAO.GetAuctionById(id);
            if (auction.Status == (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: Cuộc đấu giá này đã bị từ chối");
            }
            else if (auction.Status != (int)AuctionStatus.Assigned && auction.Status != (int)AuctionStatus.Unassigned)
            {
                throw new Exception("400: Cuộc đấu giá này đã được bàn giao");
            } 
            auction.StaffId = staffId;
            auction.Status = (int)AuctionStatus.Assigned;
            auction.UpdatedAt = DateTime.Now;
            _auctionDAO.UpdateAuction(auction);
        }

        public void ManagerApproveAuction(int id)
        {
            if (id == null)
            {
                throw new Exception("404: Không tìm thấy cuộc đấu giá");
            }
            Auction auction = GetAuctionById(id);

            if(auction.Status > (int) AuctionStatus.Pending && auction.Status != (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: Cuộc đấu giá này đã được chấp nhận");
            } 
            else if (auction.Status == (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: Cuộc đấu giá này đã bị từ chối");
            }
            else
            {                
                auction.Status = (int)AuctionStatus.Unassigned;
                auction.UpdatedAt = DateTime.Now;
                _auctionDAO.UpdateAuction(auction);
                ////Schedule task to open/end auction
                //_backgroundJobClient.Schedule(() => HostAuction(auction.Id, (int)AuctionStatus.Opened), auction.StartedAt.Value);
                //_backgroundJobClient.Schedule(() => HostAuction(auction.Id, (int)AuctionStatus.Ended), auction.EndedAt.Value);
            }
        }

        public void ManagerRejectAuction(int id)
        {
            if (id == null)
            {
                throw new Exception("404: Không tìm thấy cuộc đấu giá");
            }
            Auction auction = GetAuctionById(id);

            if (auction.Status > (int)AuctionStatus.Pending && auction.Status != (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: Cuộc đấu giá này đã được chấp nhận");
            }
            else if (auction.Status == (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: Cuộc đấu giá này đã bị từ chối");
            }
            else
            {
                auction.Status = (int)AuctionStatus.Rejected;
                auction.UpdatedAt = DateTime.Now;
                _auctionDAO.UpdateAuction(auction);
            }
        }

        public void StaffSetAuctionInfo(int id, DateTime registrationStart, DateTime registrationEnd, DateTime startedAt, DateTime endedAt,
            decimal step, int userId)
        {
            if (id == null)
            {
                throw new Exception("404: Không tìm thấy cuộc đấu giá");
            }
            Auction auction = GetAuctionById(id);
            if(auction.StaffId != userId)
            {
                throw new Exception("400: Bạn không được thay đổi thông tin cuộc đấu giá này");
            }
            if(auction.Status == (int)AuctionStatus.Pending || auction.Status == (int)AuctionStatus.Unassigned
                || auction.Status == (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: Cuộc đấu giá này không được bàn giao cho bạn");
            } 
            else if(auction.Status == (int)AuctionStatus.Opened || auction.Status == (int)AuctionStatus.Ended
                || auction.Status == (int)AuctionStatus.Unavailable)
            {
                throw new Exception("400: Bạn không thể cập nhật thông tin cuộc đấu giá này nữa");
            }
            else
            {
                auction.RegistrationStart = registrationStart;
                auction.RegistrationEnd = registrationEnd;
                auction.StartedAt = startedAt;
                auction.EndedAt = endedAt;
                auction.MaxEndedAt = endedAt.AddSeconds(90);
                auction.Status = (int)AuctionStatus.RegistrationOpen;
                auction.UpdatedAt = DateTime.Now;
                auction.Step = step;
                _auctionDAO.UpdateAuction(auction);
            }
        }

        public bool HostAuction(int auctionId, int status) {
            var auction = GetAuctionById(auctionId);

            if (auction == null) {
                return false;
            }
            DateTime startTime = (DateTime)auction.StartedAt;

            if (status == (int)AuctionStatus.Opened && DateTime.Now < startTime.AddSeconds(-5))
            {
                return false;
            }

            auction.Status = status;
            _auctionDAO.UpdateAuction(auction);
            return true;
        }

        public Bid EndAuction(int auctionId)
        {
            if (auctionId == null) 
            {
                throw new Exception("404: Không tìm thấy cuộc đấu giá");
            }
            Auction auction = GetAuctionById(auctionId);
            if (auction.Status < (int)AuctionStatus.Opened)
            {
                throw new Exception("400: Cuộc đấu giá này chưa mở");
            }
            if (auction.Status > (int)AuctionStatus.Opened)
            {
                throw new Exception("400: Cuộc đấu giá này đã kết thúc");
            }
            List<Bid> activeBids = _bidDAO.GetBidsByAuctionId(auctionId).Where(b => b.Status == (int)BidStatus.Active).ToList();
            if (activeBids.Count == 0)
            {
                auction.Status = (int)AuctionStatus.EndedWithoutBids;
                _auctionDAO.UpdateAuction(auction);

                List<Bid> registerBidList = _bidDAO.GetBidsByAuctionId(auctionId)
                            .Where(b => b.Status == (int)BidStatus.Register).ToList();

                foreach (Bid registerBid in registerBidList)
                {
                    Wallet bidderWallet = _walletDAO.Get((int)registerBid.BidderId); // lay cai wallet ra
                    bidderWallet.AvailableBalance += registerBid.BidAmount; // tra tien
                    bidderWallet.LockedBalance -= registerBid.BidAmount;

                    registerBid.Status = (int)BidStatus.Refund; // set status tra tien
                    _bidDAO.UpdateBid(registerBid);
                }

                return null;
            }
            else
            {
                auction.Status = (int)AuctionStatus.Ended;
                _auctionDAO.UpdateAuction(auction);

                var winnerBid = activeBids.OrderByDescending(b => b.BidAmount).FirstOrDefault();
                User winner = _userDAO.Get((int)winnerBid.BidderId);

                var highestBidOfEachBidder = _bidDAO.GetBidsByAuctionId(auctionId).GroupBy(b => b.BidderId) // lay het tat ca bid cao nhat cua tung thang
                    .Select(s => s.OrderByDescending(b => b.BidAmount).First())
                    .ToList();

                foreach (var bid in highestBidOfEachBidder)
                {
                    if (bid.BidderId != winner.Id)
                    {
                        Wallet bidderWallet = _walletDAO.Get((int)bid.BidderId); // lay cai wallet ra
                        bidderWallet.AvailableBalance += bid.BidAmount; // tra tien
                        bidderWallet.LockedBalance -= bid.BidAmount;

                        List<Bid> previousBidList = _bidDAO.GetBidsByAuctionId(auctionId)
                            .Where(b => b.BidderId == bid.BidderId && b.Status == (int)BidStatus.Active).ToList();

                        foreach(Bid previousBid in previousBidList)
                        {
                            previousBid.Status = (int)BidStatus.Refund; // set status tra tien
                            _bidDAO.UpdateBid(previousBid);
                        }                        

                        _walletDAO.Update(bidderWallet);
                        //_transactionDAO.Create(new Transaction()
                        //{
                        //    Id = 0,
                        //    WalletId = bidderWallet.Id,
                        //    PaymentMethod = (int)PaymentType.Wallet,
                        //    Amount = bid.BidAmount,
                        //    Type = (int)TransactionType.Refund,
                        //    Date = DateTime.Now,
                        //    Description = $"Return balance due to ending auction {auctionId}",
                        //    Status = (int)Status.Available,
                        //});
                    }    
                    else
                    {
                        Wallet winnerWallet = _walletDAO.Get((int)bid.BidderId); // lay cai wallet cua thang thang ra
                        winnerWallet.LockedBalance -= bid.BidAmount;
                        _walletDAO.Update(winnerWallet);
                        _transactionDAO.Create(new Transaction()
                        {
                            Id = 0,
                            WalletId = winnerWallet.Id,
                            PaymentMethod = (int)PaymentType.Wallet,
                            Amount = bid.BidAmount,
                            Type = (int)TransactionType.Payment,
                            Date = DateTime.Now,
                            Description = $"Thanh toán cuộc đấu giá '{auction.Title}'",
                            Status = (int)Status.Available,
                        });
                    }
                }
                return winnerBid;
            }
        }

        public bool CheckRegistration(int bidderId, int auctionId)
        {
            if(bidderId == null)
            {
                throw new Exception("400: Không tìm thấy người tham gia");
            }
            if(auctionId == null)
            {
                throw new Exception("400: Không tìm thấy cuộc đấu giá");
            }
            var checkRegistration = _bidDAO.GetBidsByAuctionId(auctionId)
                    .Where(b => b.BidderId == bidderId && b.Status == (int)BidStatus.Register)
                    .Any();
            return checkRegistration;
        }

        public async Task<int> CreateAuctionOrder(int userId, AuctionOrderRequest request) {
            var auction = _auctionDAO.GetAuctionById(request.AuctionId);
            if (auction == null) {
                throw new Exception("404: Không tìm thấy cuộc đấu giá để tạo đơn hàng");
            }
            if (auction.Status != (int) AuctionStatus.Ended) {
                throw new Exception("401: Không thể tạo đơn hàng với cuộc đấu giá này");
            }

            var address = _addressDAO.Get(request.AddressId);
            if (address == null) {
                throw new Exception("404: Không tìm thấy địa chỉ để tạo đơn hàng");
            }

            var existOrder = _orderDAO.GetByProductId(auction.ProductId.Value);
            if (existOrder != null) {
                throw new Exception("409: Không thể tạo thêm đơn hàng với cuộc đấu giá này");
            }

            var wallet = _walletService.GetByCurrentUser(userId);
            if (wallet == null) {
                throw new Exception("404: Không tìm thấy ví để tạo đơn hàng");
            }
            if (wallet.AvailableBalance < request.ShippingPrice) {
                throw new Exception("401: Ví của bạn không có đủ số dư để tạo đơn hàng");
            }

            var now = DateTime.Now;
            var highestBid = _bidDAO.GetBidsByAuctionId(request.AuctionId).OrderByDescending(b => b.BidAmount).FirstOrDefault();

            var order = new Order {
                BuyerId = userId,
                SellerId = auction.Product.SellerId,
                RecipientName = address.RecipientName,
                RecipientPhone = address.RecipientPhone,
                RecipientAddress = address.Street + ", " + address.Ward + ", " + address.District + ", " + address.Province,
                OrderDate = now,
                TotalAmount = highestBid.BidAmount,
                ShippingCost = request.ShippingPrice,
                Status = (int) OrderStatus.ReadyForPickup,
                OrderItems = new List<OrderItem>()
            };

            order.OrderItems.Add(new OrderItem {
                ProductId = auction.ProductId.Value,
                Price = highestBid.BidAmount,
                Quantity = 1,
                ImageUrl = _productImageDAO.GetByProductId((int)auction.ProductId).FirstOrDefault().ImageUrl
            });
            _orderDAO.Create(order);

            var seller = _sellerDAO.GetSeller(order.SellerId.Value);
            if (seller == null)
            {
                throw new Exception("404: Không tìm thấy người bán để tạo đơn vận chuyển");
            }

            var sellerAddress = _addressDAO.GetPickupBySellerId(order.SellerId.Value);
            if (sellerAddress == null)
            {
                throw new Exception("404: Không tìm thấy địa chỉ người bán để tạo đơn vận chuyển");
            }
            if (sellerAddress.Type != (int)AddressType.Pickup)
            {
                throw new Exception("401: Địa chỉ hiện tại không phải địa chỉ lấy hàng");
            }

            var buyerAddress = _addressDAO.GetBuyerAddressInOrder(order.BuyerId.Value, order.RecipientAddress);
            if (buyerAddress == null)
            {
                throw new Exception("404: Không tìm thấy địa chỉ để tạo đơn vận chuyển");
            }

            //Call to GHN to create shipping order
            var client = _httpClientFactory.CreateClient("GHN");
            client.DefaultRequestHeaders.Add("shop_id", seller.ShopId);
            var requestGHN = new ShippingOrderRequest
            {
                note = $"Đơn hàng cho khách hàng {buyerAddress.RecipientName}",
                return_name = sellerAddress.RecipientName,
                return_address = sellerAddress.Street,
                return_ward_code = sellerAddress.WardCode,
                return_district_id = sellerAddress.DistrictId.Value,
                return_phone = sellerAddress.RecipientPhone,
                to_name = buyerAddress.RecipientName,
                to_phone = buyerAddress.RecipientPhone,
                to_address = buyerAddress.Street,
                to_district_id = buyerAddress.DistrictId.Value,
                to_ward_code = buyerAddress.WardCode,
                weight = 0,
                items = new List<ShippingOrderItem>()
            };
            order.OrderItems.ToList().ForEach(p => {
                var product = _productDAO.GetProductById(p.ProductId);
                requestGHN.items.Add(new ShippingOrderItem
                {
                    name = product.Name,
                    code = product.Id.ToString(),
                    quantity = p.Quantity.Value,
                    weight = (int)product.Weight.Value
                });
                requestGHN.weight += (int)product.Weight.Value;
            });

            var responseMessage = await client.PostAsync("v2/shipping-order/create", Utils.ConvertForPost<ShippingOrderRequest>(requestGHN));
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"400: {responseMessage.Content.ReadAsStringAsync().Result}");
            }
            var data = await responseMessage.Content.ReadAsAsync<BaseGHNResponse<ShippingOrderResponse>>();
            order.OrderShippingCode = data.data.order_code;
            order.Status = (int)OrderStatus.ReadyForPickup;
            _orderDAO.UpdateOrder(order);

            //Check order status
            await CheckStatusShippingOrder(order.Id, data.data.order_code);
            return order.Id;
        }

        public async Task CheckStatusShippingOrder(int orderId, string orderShippingCode)
        {
            //Call to GHN
            var client = _httpClientFactory.CreateClient("GHN");
            StringContent content = new(
                JsonSerializer.Serialize(new
                {
                    order_code = orderShippingCode
                }),
                Encoding.UTF8,
                "application/json");
            var httpResponse = await client.PostAsync("v2/shipping-order/detail", content);
            if (!httpResponse.IsSuccessStatusCode)
            {
                _backgroundJobClient.Schedule(() => CheckStatusShippingOrder(orderId, orderShippingCode), DateTime.Now.AddMinutes(10));
                return;
            }

            //Get data
            var responseData = await httpResponse.Content.ReadAsAsync<ShippingOrder.Root>();
            var order = _orderDAO.Get(orderId);
            if (order == null)
            {
                throw new Exception("404: Không tìm thấy đơn hàng để cập nhật sau khi vận chuyển");
            }

            if (responseData.data.log == null)
            {
                _backgroundJobClient.Schedule(() => CheckStatusShippingOrder(orderId, orderShippingCode), DateTime.Now.AddMinutes(60 * 24));
                return;
            }

            //Ready for pickup => Delivering
            if (order.Status == (int)OrderStatus.ReadyForPickup)
            {
                var log = responseData.data.log.Where(p => p.status.Contains("picked"));
                if (log.Any())
                {
                    order.Status = (int)OrderStatus.Delivering;
                    _orderDAO.UpdateOrder(order);
                }
                _backgroundJobClient.Schedule(() => CheckStatusShippingOrder(orderId, orderShippingCode), DateTime.Now.AddMinutes(60 * 24));
                return;
            }

            //Delivering => Delivered
            if (order.Status == (int)OrderStatus.Delivering)
            {
                var log = responseData.data.log.Where(p => p.status.Contains("delivered"));
                if (log.Any())
                {
                    order.Status = (int)OrderStatus.Delivered;
                    _orderDAO.UpdateOrder(order);
                    _backgroundJobClient.Schedule(() => DoneOrder(orderId), DateTime.Now.AddMinutes(60 * 24));
                }
                return;
            }
        }

        public async void DoneOrder(int orderId)
        {
            var order = _orderDAO.Get(orderId);
            if (order == null)
            {
                throw new Exception("404: Không tìm thấy đơn hàng để cập nhật hoàn tất");
            }
            if (order.Status == (int)OrderStatus.Done)
            {
                return;
            }

            order.Status = (int)OrderStatus.Done;
            _orderDAO.UpdateOrder(order);
            _walletService.AddSellerBalance(orderId);
        }

        public bool CheckWinner(int bidderId, int auctionId)
        {
            Auction auction = _auctionDAO.GetAuctionById(auctionId);
            Product product = auction.Product;
            var checkOrderExist = _orderDAO.GetAllOrder().Where(o => o.OrderItems.Any(i => i.ProductId == product.Id)).ToList();
            if(!CheckRegistration(bidderId, auctionId))
            {
                return false;
            }
            List<Bid> activeBids = _bidDAO.GetBidsByAuctionId(auctionId)
                .Where(b => b.Status == (int)BidStatus.Active).ToList();
            if (activeBids.Count == 0)
            {
                return false;
            }
            if (checkOrderExist.Count > 0)
            {
                return false;
            }
            Bid winnerBid = activeBids.OrderByDescending(b => b.BidAmount).FirstOrDefault();
            if(winnerBid.BidderId == bidderId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public void TestSchedule() {
        //    var auction = _auctionDAO.GetAuctionById(3);

        //    auction.StartedAt = DateTime.Now.AddMinutes(1);
        //    auction.EndedAt = DateTime.Now.AddMinutes(2);
        //    _auctionDAO.UpdateAuction(auction);
        //_backgroundJobClient.Schedule(() => HostAuction(auction.Id, (int) AuctionStatus.Opened), auction.StartedAt.Value);
        //    _backgroundJobClient.Schedule(() => HostAuction(auction.Id, (int) AuctionStatus.Ended), auction.EndedAt.Value);
        //}

        //public void OpenAuction(int id)
        //{
        //    Auction auction = GetAuctionById(id);
        //    var status = auction.Status;
        //    switch (status)
        //    {
        //        case (int) AuctionStatus.Approved:
        //            if (auction.StartedAt == DateTime.Now)
        //            {
        //                auction.Status = (int)AuctionStatus.Opened;
        //                auction.UpdatedAt = DateTime.Now;
        //                _auctionDAO.UpdateAuction(auction);
        //            }                    
        //            break;

        //        case (int)AuctionStatus.Pending:
        //            throw new Exception("400: This auction is unapproved.");

        //        case (int)AuctionStatus.Unassigned:
        //            throw new Exception("400: This auction is unassigned.");

        //        case (int)AuctionStatus.Rejected:
        //            throw new Exception("400: This auction is rejected.");

        //        case (int)AuctionStatus.Opened:
        //            throw new Exception("400: This auction is opening.");

        //        default: throw new Exception("400: This auction is ended.");
        //    }
        //}

        //public void EndAuction(int id)
        //{
        //    Auction auction = GetAuctionById(id);
        //    var status = auction.Status;

        //    if (auction.Status == (int)AuctionStatus.Opened)
        //    {
        //        throw new Exception("400: This auction is opening.");
        //    }
        //    else if (auction.Status == (int)AuctionStatus.Ended
        //        || auction.Status == (int)AuctionStatus.Sold
        //        || auction.Status == (int)AuctionStatus.Expired)
        //    {
        //        throw new Exception("400: This auction is ended.");
        //    }
        //    else
        //    {
        //        throw new Exception("400: This auction cannot be rejected.");
        //    }
        //}
    }
}

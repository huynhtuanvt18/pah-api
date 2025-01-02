using DataAccess;
using DataAccess.Models;
using Firebase.Auth;
using Hangfire;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Request;
using Request.ThirdParty.GHN;
using Request.ThirdParty.Zalopay;
using Service.EmailService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.Implement {
    public class OrderService : IOrderService {
        private readonly IOrderDAO _orderDAO;
        private readonly IProductDAO _productDAO;
        private readonly IAddressDAO _addressDAO;
        private readonly IProductImageDAO _productImageDAO;
        private readonly IOrderCancelDAO _orderCancelDAO;
        private readonly IWalletService _walletService;
        private readonly ISellerDAO _sellerDAO;
        private readonly IUserDAO _userDAO;
        private readonly IConfiguration _config;
        private IHttpClientFactory _httpClientFactory;
        private IBackgroundJobClient _backgroundJobClient;

        public OrderService(IOrderDAO orderDAO, 
            IProductDAO productDAO, IAddressDAO addressDAO, 
            IProductImageDAO productImageDAO, IOrderCancelDAO orderCancelDAO,
            IWalletService walletService, IConfiguration config,
            IHttpClientFactory httpClientFactory, IBackgroundJobClient backgroundJobClient,
            ISellerDAO sellerDAO, IUserDAO userDAO) {
            _orderDAO = orderDAO;
            _productDAO = productDAO;
            _addressDAO = addressDAO;
            _productImageDAO = productImageDAO;
            _orderCancelDAO = orderCancelDAO;
            _walletService = walletService;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _backgroundJobClient = backgroundJobClient;
            _sellerDAO = sellerDAO;
            _userDAO = userDAO;
        }

        public void SellerCancelOrder(int sellerId, int orderId, string reason) {
            var order = _orderDAO.Get(orderId);

            if (order == null) throw new Exception("404: Không tìm thấy đơn hàng");
            if (sellerId != order.SellerId || order.Status != (int) OrderStatus.WaitingSellerConfirm) throw new Exception("401: Bạn không được quyền hủy đơn hàng này");

            order.Status = (int) OrderStatus.CancelledBySeller;
            _orderDAO.UpdateOrder(order);
            _walletService.RefundOrder(order.Id);
            _orderCancelDAO.Create(new OrderCancellation { Id = order.Id, Reason = reason });
        }

        public void ApproveCancelOrderRequest(int sellerId, int orderId) {
            var order = _orderDAO.Get(orderId);

            if (order == null) throw new Exception("404: Không tìm thấy đơn hàng");
            if (sellerId != order.SellerId || order.Status != (int) OrderStatus.CancelApprovalPending) throw new Exception("401: Bạn không được quyền chấp nhận yêu cầu hủy đơn hàng này");

            order.Status = (int) OrderStatus.CancelledByBuyer;
            _orderDAO.UpdateOrder(order);
            _walletService.RefundOrder(order.Id);
            _orderCancelDAO.Create(new OrderCancellation { Id = order.Id, Reason = "Người mua đã hủy đơn hàng" });
        }

        public void CancelOrderRequest(int buyerId, int orderId) {
            var order = _orderDAO.Get(orderId);

            if (order == null) throw new Exception("404: Không tìm thấy đơn hàng");
            if (buyerId != order.BuyerId || order.Status != (int) OrderStatus.WaitingSellerConfirm) throw new Exception($"401: Bạn không được quyền hủy đơn hàng với trạng thái là: {order.Status}");

            order.Status = (int) OrderStatus.CancelApprovalPending;
            _orderDAO.UpdateOrder(order);
        }

        public void Create(Order order) {
            _orderDAO.Create(order);
        }

        public void Checkout(CheckoutRequest request, int buyerId, int addressId) {
            var address = _addressDAO.Get(addressId);
            if (address == null) throw new Exception("404: Không tìm thấy địa chỉ để mua hàng");
            if (address.CustomerId != buyerId) throw new Exception("401: Địa chỉ không thống nhất với người mua hiện tại");
            //if (address.Type != (int) AddressType.Delivery) throw new Exception("401: Address type must be delivery");

            var dateNow = DateTime.Now;
            var totalWithShip = request.Total;
            request.Order.ForEach(p => totalWithShip += p.ShippingCost);

            var wallet = _walletService.GetByCurrentUser(buyerId);
            if (wallet == null) {
                throw new Exception("400: Không tìm thấy ví");
            }
            if (wallet.AvailableBalance < totalWithShip && request.PaymentType == (int)PaymentType.Wallet) {
                throw new Exception("400: Không đủ số dư để tạo đơn hàng");
            }

            foreach (var order in request.Order) {
                List<Product> products = new List<Product>();
                //totalWithShip += order.ShippingCost;
                
                Order insert = new Order {
                    BuyerId = buyerId,
                    SellerId = order.SellerId,
                    RecipientName = address.RecipientName,
                    RecipientPhone = address.RecipientPhone,
                    RecipientAddress = address.Street + ", " + address.Ward + ", " + address.District + ", " + address.Province,
                    OrderDate = dateNow,
                    Status = (int) OrderStatus.WaitingSellerConfirm,
                    ShippingCost = order.ShippingCost,
                    TotalAmount = order.Total,
                    OrderItems = new List<OrderItem>()
                };

                foreach (var product in order.Products) {
                    var dbProduct = _productDAO.GetProductById(product.Id);
                    if (dbProduct == null) {
                        throw new Exception("404: Không tìm thấy sản phẩm để tạo đơn hàng");
                    }
                    if (!CheckPrice(product.Price, dbProduct.Price)) {
                        throw new Exception("400: Giá sản phẩm hiện tại không khớp với giá của cơ sở dữ liệu");
                    }
                    if (order.SellerId != dbProduct.SellerId) {
                        throw new Exception("400: Người bán hiện tại không khớp với người bán của sản phẩm trong cơ sở dữ liệu");
                    }
                    if(dbProduct.SellerId == buyerId)
                    {
                        throw new Exception("404: Người bán không được phép mua sản phẩm của chính mình");
                    }
                    insert.OrderItems.Add(new OrderItem {
                        ProductId = dbProduct.Id,
                        Price = dbProduct.Price,
                        Quantity = product.Amount,
                        ImageUrl = _productImageDAO.GetByProductId(dbProduct.Id).FirstOrDefault().ImageUrl
                    });
                    //insert.TotalAmount += dbProduct.Price * product.Amount;
                }
                _orderDAO.Create(insert);
            }
            switch (request.PaymentType) {
                case (int)PaymentType.Wallet:
                    CheckoutWallet(totalWithShip, buyerId, dateNow); 
                    break;
                case (int) PaymentType.Zalopay:
                    CheckoutZalopay(totalWithShip, buyerId, dateNow); 
                    break;
                default:
                    break;
            }
        }

        private void CheckoutWallet(decimal total, int buyerId, DateTime now) {
            var orderList = _orderDAO.GetAllByBuyerIdAfterCheckout(buyerId, now).ToList();
            orderList.ForEach(order => {
                _walletService.CheckoutWallet(buyerId, order.Id, (int) OrderStatus.WaitingSellerConfirm);
            });
        }

        private void CheckoutZalopay(decimal total, int buyerId, DateTime now) {
            var orderList = _orderDAO.GetAllByBuyerIdAfterCheckout(buyerId, now).ToList();
            orderList.ForEach(order => {
                _walletService.CheckoutZalopay(buyerId, order.Id, (int)OrderStatus.WaitingSellerConfirm);
            });
        }

        private bool CheckPrice(decimal requestPrice, decimal dbPrice) {
            return requestPrice == dbPrice;
        }

        public List<Order> GetAll(int status) {
            var a = status ==  0 ? _orderDAO.GetAllOrder().ToList() : _orderDAO.GetAllOrder().Where(p => p.Status==status).ToList();
            a.ForEach(p => p.OrderItems.ToList().ForEach(p => { p.Product = _productDAO.GetProductById(p.ProductId); }));
            return a;
        }

        public List<Order> GetByBuyerId(int buyerId, List<int> status) {
            var a = status.Count == 0 ? _orderDAO.GetAllByBuyerId(buyerId).ToList() : _orderDAO.GetAllByBuyerId(buyerId).Where(p => status.Contains(p.Status)).ToList();
            a.ForEach(p => p.OrderItems.ToList().ForEach(p => { p.Product = _productDAO.GetProductById(p.ProductId); }));
            return a;
        }

        public List<Order> GetBySellerId(int sellerId, List<int> status) {
            var a = status.Count == 0 ? _orderDAO.GetAllBySellerId(sellerId).ToList() : _orderDAO.GetAllBySellerId(sellerId).Where(p => status.Contains(p.Status)).ToList();
            a.ForEach(p => p.OrderItems.ToList().ForEach(p => { p.Product = _productDAO.GetProductById(p.ProductId); }));
            return a;
        }

        public List<Order> GetProcessingBySellerId(int sellerId)
        {
            return _orderDAO.GetAllBySellerId(sellerId).Where(o => o.Status < (int)OrderStatus.Done || o.Status == (int)OrderStatus.CancelApprovalPending).ToList();
        }

        public Order UpdateOrderStatus(int sellerId, int status, int orderId) {
            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Không tìm thấy đơn hàng");
            }

            if (sellerId !=  order.SellerId 
                || order.Status == (int)OrderStatus.CancelledByBuyer 
                || order.Status == (int) OrderStatus.CancelledBySeller) {
                throw new Exception("401: Bạn không được quyền cập nhật đơn hàng này");
            }

            order.Status = status;
            _orderDAO.UpdateOrder(order);
            return _orderDAO.Get(orderId);
        }

        public Order Get(int orderId) {
            var order = _orderDAO.Get(orderId);
            order.OrderItems.ToList().ForEach(p => { p.Product = _productDAO.GetProductById(p.ProductId); });
            return order;
        }

        public async Task DefaultShippingOrder(int orderId) {
            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Không tìm thấy đơn hàng để tạo đơn vận chuyển");
            }

            if (order.Status != (int) OrderStatus.ReadyForPickup) {
                throw new Exception("401: Đơn hàng này không thể được bàn giao để vận chuyển");
            }
            string orderShippingCode = _config["API3rdParty:GHN:dev:defaultShippingCode"];
            order.OrderShippingCode = orderShippingCode;
            _orderDAO.UpdateOrder(order);
            await CheckStatusShippingOrder(orderId, orderShippingCode);
        }

        public async Task CheckStatusShippingOrder(int orderId, string orderShippingCode) {
            //Call to GHN
            var client = _httpClientFactory.CreateClient("GHN");
            StringContent content = new(
                JsonSerializer.Serialize(new {
                    order_code = orderShippingCode
                }),
                Encoding.UTF8,
                "application/json");
            var httpResponse = await client.PostAsync("v2/shipping-order/detail", content);
            if (!httpResponse.IsSuccessStatusCode) {
                _backgroundJobClient.Schedule(() => CheckStatusShippingOrder(orderId, orderShippingCode), DateTime.Now.AddMinutes(10));
                return;
            }

            //Get data
            var responseData = await httpResponse.Content.ReadAsAsync<ShippingOrder.Root>();
            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Không tìm thấy đơn hàng để cập nhật sau khi vận chuyển");
            }

            if (responseData.data.log == null) {
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
            if (order.Status == (int) OrderStatus.Delivering) {
                var log = responseData.data.log.Where(p => p.status.Contains("delivered"));
                if (log.Any()) {
                    order.Status = (int) OrderStatus.Delivered;
                    _orderDAO.UpdateOrder(order);
                    _backgroundJobClient.Schedule(() => DoneOrder(orderId), DateTime.Now.AddMinutes(60 * 24));
                }
                return;
            }
        }

        public async void DoneOrder(int orderId) {
            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Không tìm thấy đơn hàng để cập nhật hoàn tất");
            }
            if (order.Status == (int) OrderStatus.Done) {
                return;
            }

            order.Status = (int) OrderStatus.Done;
            _orderDAO.UpdateOrder(order);
            _walletService.AddSellerBalance(orderId);
        }

        public async Task CreateShippingOrder(int orderId) {
            //Get order, address info
            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Không tìm thấy đơn hàng để tạo đơn vận chuyển");
            }
            if (order.Status != (int) OrderStatus.WaitingSellerConfirm) {
                throw new Exception("401: Không thể tạo đơn vận chuyển cho đơn hàng này");
            }

            var seller = _sellerDAO.GetSeller(order.SellerId.Value);
            if (seller == null) {
                throw new Exception("404: Không tìm thấy người bán để tạo đơn vận chuyển");
            }
            
            var sellerAddress = _addressDAO.GetPickupBySellerId(order.SellerId.Value);
            if (sellerAddress == null) {
                throw new Exception("404: Không tìm thấy địa chỉ người bán để tạo đơn vận chuyển");
            }
            if (sellerAddress.Type != (int) AddressType.Pickup) {
                throw new Exception("401: Địa chỉ hiện tại không phải địa chỉ lấy hàng");
            }

            var buyerAddress = _addressDAO.GetBuyerAddressInOrder(order.BuyerId.Value, order.RecipientAddress);
            if (buyerAddress == null) {
                throw new Exception("404: Không tìm thấy địa chỉ để tạo đơn vận chuyển");
            }

            //Call to GHN to create shipping order
            var client = _httpClientFactory.CreateClient("GHN");
            client.DefaultRequestHeaders.Add("shop_id", seller.ShopId);
            var request = new ShippingOrderRequest {
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
                items = new List<ShippingOrderItem>(),
                insurance_value = 0
            };
            order.OrderItems.ToList().ForEach(p => {
                var product = _productDAO.GetProductById(p.ProductId);
                request.items.Add(new ShippingOrderItem {
                    name = product.Name,
                    code = product.Id.ToString(),
                    quantity = p.Quantity.Value,
                    weight = (int) product.Weight.Value
                });
                request.weight += (int) product.Weight.Value;
            });
            
            var responseMessage = await client.PostAsync("v2/shipping-order/create", Utils.ConvertForPost<ShippingOrderRequest>(request));
            if (!responseMessage.IsSuccessStatusCode) {
                throw new Exception($"400: {responseMessage.Content.ReadAsStringAsync().Result}");
            }
            var data = await responseMessage.Content.ReadAsAsync<BaseGHNResponse<ShippingOrderResponse>>();
            order.OrderShippingCode = data.data.order_code;
            order.Status = (int) OrderStatus.ReadyForPickup;
            _orderDAO.UpdateOrder(order);

            //Check order status
            await CheckStatusShippingOrder(orderId, data.data.order_code);
        }
    }
}

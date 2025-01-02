using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Request.ThirdParty.Zalopay;
using Request;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Service.Implement
{
    public class WalletService : IWalletService
    {
        private readonly IWalletDAO _walletDAO;
        private readonly ITransactionDAO _transactionDAO;
        private readonly IOrderDAO _orderDAO;
        private readonly IWithdrawalDAO _withdrawalDAO;
        private readonly IHttpClientFactory _httpClientFactory;

        public WalletService(IWalletDAO walletDAO, ITransactionDAO transactionDAO, IHttpClientFactory httpClientFactory, IOrderDAO orderDAO, IWithdrawalDAO withdrawalDAO) {
            _walletDAO = walletDAO;
            _transactionDAO = transactionDAO;
            _httpClientFactory = httpClientFactory;
            _orderDAO = orderDAO;
            _withdrawalDAO = withdrawalDAO;
        }

        public async Task Topup(int userId, TopupRequest topupRequest) {
            var wallet = _walletDAO.Get(userId);
            if (wallet == null) {
                throw new Exception("404: Không tìm thấy ví");
            }

            //Check transaction from zalopay in committed in db
            if (!_transactionDAO.IsZalopayOrderValid(topupRequest.AppTransId, topupRequest.Mac)) {
                throw new Exception("409: Đơn hàng từ ZaloPay không hợp lệ");
            }

            //Uncomment when zalopay fix their api

            ////Call to zalopay
            //var httpClient = _httpClientFactory.CreateClient("Zalopay");
            //var data = new QueryRequest {
            //    app_id = orderRequest.AppId,
            //    app_trans_id = orderRequest.AppTransId,
            //    mac = orderRequest.Mac
            //};
            //var httpResponseMessage = await httpClient.PostAsync("query", Utils.ConvertForPost<QueryRequest>(data));

            //if (!httpResponseMessage.IsSuccessStatusCode) {
            //    throw new Exception("400: No order in zalo pay yet");
            //}

            ////Validation
            //var responseData = await httpResponseMessage.Content.ReadAsAsync<QueryResponse>();
            //if (responseData.return_code != 1) {
            //    throw new Exception("400: " + responseData.return_message);
            //}
            //if (responseData.amount != orderRequest.Topup) {
            //    throw new Exception("400: Amount does not match with order from zalopay");
            //}

            //Topup and create transaction
            wallet.AvailableBalance += topupRequest.Topup;
            _walletDAO.Update(wallet);
            _transactionDAO.Create(new DataAccess.Models.Transaction {
                Id = 0,
                WalletId = wallet.Id,
                PaymentMethod = (int) PaymentType.Zalopay,
                Amount = topupRequest.Topup,
                Type = (int) TransactionType.Deposit,
                Date = DateTime.Now,
                //Description = $"app_id: {topupRequest.AppId}, " +
                //    $"app_trans_id: {topupRequest.AppTransId}, " +
                //    $"mac: {topupRequest.Mac}",
                Description = $"Nạp tiền vào ví PAH từ ZaloPay. Mã giao dịch: {topupRequest.AppTransId}",
                Status = (int) Status.Available
            });
        }

        public void AddLockedBalance(int userId, decimal balance) {
            throw new NotImplementedException();
        }

        public void SubtractLockedBalance(int userId, decimal balance) {
            throw new NotImplementedException();
        }

        public void CheckoutWallet(int userId, int orderId, int orderStatus) {
            var wallet = _walletDAO.Get(userId);
            if (wallet == null) {
                throw new Exception("404: Không tìm thấy ví");
            }

            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Không tìm thấy đơn hàng để thanh toán");
            }

            if (wallet.AvailableBalance < order.TotalAmount + order.ShippingCost) {
                throw new Exception($"400: Không đủ số dư trong ví để xác nhận đơn hàng: {orderId}");
            }

            //Subtract from wallet, create transaction, update order
            wallet.AvailableBalance -= (order.TotalAmount + order.ShippingCost);
            _walletDAO.Update(wallet);
            _transactionDAO.Create(new DataAccess.Models.Transaction {
                Id = 0,
                WalletId = wallet.Id,
                PaymentMethod = (int) PaymentType.Wallet,
                Amount = order.TotalAmount + order.ShippingCost,
                Type = (int) TransactionType.Payment,
                Date = DateTime.Now,
                Description = $"Thanh toán đơn hàng: {orderId}",
                Status = (int) Status.Available
            });
            //Update order status to Waiting for Seller Confirm
            order.Status = orderStatus;
            _orderDAO.UpdateOrder(order);
        }
        
        public void CheckoutZalopay(int userId, int orderId, int orderStatus) {
            var wallet = _walletDAO.Get(userId);
            if (wallet == null) {
                throw new Exception("404: Không tìm thấy ví");
            }

            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Không tìm thấy đơn hàng để thanh toán");
            }

            //Add transaction
            _transactionDAO.Create(new DataAccess.Models.Transaction {
                Id = 0,
                WalletId = wallet.Id,
                PaymentMethod = (int) PaymentType.Zalopay,
                Amount = order.TotalAmount + order.ShippingCost,
                Type = (int) TransactionType.Payment,
                Date = DateTime.Now,
                Description = $"Thanh toán đơn hàng: {orderId}",
                Status = (int) Status.Available
            });
            //Update order status to Waiting for Seller Confirm
            order.Status = orderStatus;
            _orderDAO.UpdateOrder(order);
        }

        public Wallet GetByCurrentUser(int id)
        {
            return _walletDAO.GetByCurrentUser(id);
        }

        public void RefundOrder(int orderId) {
            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Không tìm thấy đơn hàng để hoàn tiền");
            }
            if (order.Status != (int) OrderStatus.CancelledByBuyer && order.Status != (int) OrderStatus.CancelledBySeller) {
                throw new Exception("401: Đơn hàng chưa được hủy, không thể hoàn tiền");
            }

            var wallet = _walletDAO.Get(order.BuyerId.Value);
            if (wallet == null) {
                throw new Exception("404: Không tìm thấy ví để hoàn tiền");
            }
            var amount = order.ShippingCost + order.TotalAmount;
            wallet.AvailableBalance += amount;
            _walletDAO.Update(wallet);
            _transactionDAO.Create(new Transaction {
                Id = 0,
                WalletId = wallet.Id,
                PaymentMethod = (int) PaymentType.Wallet,
                Amount = amount,
                Type = (int) TransactionType.Refund,
                Date = DateTime.Now,
                Description = $"Hoàn tiền cho đơn hàng: {orderId}",
                Status = (int) Status.Available
            });
        }

        public void AddSellerBalance(int orderId) {
            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Không tìm thấy đơn hàng để thanh toán tiền cho người bán");
            }
            if (order.Status != (int) OrderStatus.Done) {
                throw new Exception("401: Đơn hàng chưa hoàn tất, không thể thanh toán cho người bán");
            }

            var wallet = _walletDAO.Get(order.SellerId.Value);
            if (wallet == null) {
                throw new Exception("404: Không tìm thấy ví để thanh toán cho người bán");
            }
            var amount = order.ShippingCost + order.TotalAmount * .97m ;
            wallet.AvailableBalance += amount;
            _walletDAO.Update(wallet);
            _transactionDAO.Create(new Transaction {
                Id = 0,
                WalletId = wallet.Id,
                PaymentMethod = (int) PaymentType.Wallet,
                Amount = amount,
                Type = (int) TransactionType.DoneOrder,
                Date = DateTime.Now,
                Description = $"Done for order: {orderId}",
                Status = (int) Status.Available
            });
        }

        public void CreateWithdrawal(int userId, WithdrawalRequest request) {
            var wallet = _walletDAO.Get(userId);
            if (wallet == null) {
                throw new Exception("404: Không tìm thấy ví để tạo yêu cầu rút tiền");
            }

            if (wallet.AvailableBalance < request.Amount) {
                throw new Exception("401: Ví không đủ số dư để tạo yêu cầu rút tiền");
            }

            wallet.AvailableBalance -= request.Amount;
            wallet.LockedBalance += request.Amount;
            _walletDAO.Update(wallet);

            var withdrawal = new Withdrawal {
                Amount = request.Amount,
                WalletId = wallet.Id,
                Bank = request.Bank,
                BankNumber = request.BankNumber,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Status = (int) WithdrawalStatus.Pending
            };
            _withdrawalDAO.Create(withdrawal);
        }

        public void ApproveWithdrawal(int withdrawalId, int managerId) {
            var withdrawal = _withdrawalDAO.Get(withdrawalId);
            if (withdrawal == null) {
                throw new Exception("404: Không tìm thấy yêu cầu rút tiền");
            }

            var wallet = _walletDAO.Get(withdrawal.WalletId);
            if (wallet == null) {
                throw new Exception("404:: Không tìm thấy ví để xử lí yêu cầu rút tiền");
            }
            if (wallet.LockedBalance < withdrawal.Amount) {
                throw new Exception("401: Ví không đủ số dư để rút tiền");
            }

            wallet.LockedBalance -= withdrawal.Amount;
            _walletDAO.Update(wallet);

            var transaction = new Transaction {
                WalletId = wallet.Id,
                Amount = withdrawal.Amount,
                Date = DateTime.Now,
                PaymentMethod = (int) PaymentType.Wallet,
                Type = (int) TransactionType.Withdraw,
                Description = $"Rút về số ngân hàng: {withdrawal.BankNumber} với số tiền: {withdrawal.Amount}",
                Status = (int) Status.Available
            };
            _transactionDAO.Create(transaction);

            withdrawal.Status = (int) WithdrawalStatus.Done;
            withdrawal.ManagerId = managerId;
            withdrawal.UpdatedAt = DateTime.Now;
            _withdrawalDAO.Update(withdrawal);
        }

        public void DenyWithdrawal(int withdrawalId, int managerId) {
            var withdrawal = _withdrawalDAO.Get(withdrawalId);
            if (withdrawal == null) {
                throw new Exception("404: Không tìm thấy yêu cầu rút tiền");
            }

            var wallet = _walletDAO.Get(withdrawal.WalletId);
            if (wallet == null)
            {
                throw new Exception("404: Không tìm thấy ví để xử lí yêu cầu rút tiền");
            }
            if (wallet.LockedBalance < withdrawal.Amount)
            {
                throw new Exception("401: Ví không đủ số dư để rút tiền");
            }

            wallet.LockedBalance -= withdrawal.Amount;
            wallet.AvailableBalance += withdrawal.Amount;
            _walletDAO.Update(wallet);

            withdrawal.Status = (int) WithdrawalStatus.Rejected;
            withdrawal.UpdatedAt = DateTime.Now;
            withdrawal.ManagerId = managerId;
            _withdrawalDAO.Update(withdrawal);
        }

        public List<Withdrawal> GetWithdrawalByUserId(int userId) {
            return _withdrawalDAO.GetByUserId(userId).OrderByDescending(w => w.CreatedAt).ToList();
        }

        public List<Withdrawal> GetWithdrawalManager()
        {
            return _withdrawalDAO.GetAll().OrderByDescending(w => w.CreatedAt).ToList();
        }
    }
}

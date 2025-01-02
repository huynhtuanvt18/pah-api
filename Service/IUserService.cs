using DataAccess.Models;
using Request;
using Respon.UserRes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service {
    public interface IUserService {
        public User Get(int id);
        public List<User> GetAll();
        public List<User> GetAllBuyersSellers();
        public List<User> GetAllStaffs();
        public List<User> GetBuyersWithDoneOrders();
        public User GetByEmail(string email);
        public User Login(string email, string password);
        public User LoginWithGoogle(string email, string name, string profileUrl);

        public void Register(User user);
        public void UpdateProfile(int id, UpdateProfileRequest request);
        public void Reactivate(User user);
        public void Deactivate(User user);  
        public void AcceptSeller(Seller seller);
        public void RejectSeller(Seller seller);

        public List<User> GetAvailableStaffs();
        public List<User> GetReactivateRequestList();

        public Tokens AddRefreshToken(int id);
        public Token GetSavedRefreshToken(int id, string refreshToken);
        public void RemoveRefreshToken(int id);

        public void AddResetToken(int id, string token);
        public void ChangePassword(ChangePasswordRequest request, string email);
        public void ResetPassword(ResetPasswordRequest request);
        public void VerifyAccount(string email, string code);
        public string CreateVerificationCode(string email);
        public List<RevenueResponse> GetRevenues(int year);
        public int CountDoneOrdersByBuyerId(int id);
    }
}

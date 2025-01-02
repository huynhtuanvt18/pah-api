using DataAccess;
using DataAccess.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using Request.Param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement {
    public class AdminService : IAdminService {
        private readonly IUserDAO _userDAO;
        private readonly int WORK_FACTOR = 13;
        private static readonly string DEFAULT_AVT = "https://static.vecteezy.com/system/resources/thumbnails/001/840/618/small/picture-profile-icon-male-icon-human-or-people-sign-and-symbol-free-vector.jpg";
        public AdminService(IUserDAO userDAO) {
            _userDAO = userDAO;
        }

        public void CreateStaff(User user) {
            var dbUser = _userDAO.GetByEmail(user.Email);
            if (dbUser != null ) {
                throw new Exception("409: Địa chỉ email đã tồn tại");
            }
            user.Password = BC.EnhancedHashPassword(user.Password, WORK_FACTOR);
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            //user.Role = (int) Role.Staff;
            user.Status = (int) Status.Unverified;
            user.ProfilePicture = DEFAULT_AVT;
            _userDAO.Register(user);
        }

        public void UpdateStaff(User user) {
            var dbUser = _userDAO.GetByEmail(user.Email);
            if ( dbUser == null ) {
                throw new Exception("404: Không tìm thấy nhân viên");
            }
            if (dbUser.Role != (int) Role.Staff) {
                throw new Exception("401: Không thể cập nhật thông tin nhân viên này");
            }
            dbUser.Name = user.Name;
            dbUser.Email = user.Email;
            dbUser.Password = BC.EnhancedHashPassword(user.Password, WORK_FACTOR);
            dbUser.Phone = user.Phone;
            dbUser.ProfilePicture = user.ProfilePicture == null ? DEFAULT_AVT : user.ProfilePicture;
            dbUser.Gender = user.Gender;
            dbUser.Dob = user.Dob;
            dbUser.Role = user.Role;
            dbUser.Status = user.Status;
            dbUser.UpdatedAt = DateTime.Now;
            _userDAO.Update(dbUser);
        }

        public List<User> GetAccounts(AccountParam accountParam) {
            var list = _userDAO.GetAll();
            if ( accountParam.Name != null ) {
                list = list.Where(p => p.Name.Contains(accountParam.Name));
            }
            if ( accountParam.Status != null ) {
                list = list.Where(p => p.Status == accountParam.Status);
            }
            if ( accountParam.Role != null ) {
                list = list.Where(p => p.Role == accountParam.Role);
            }
            if ( accountParam.Gender != null ) {
                list = list.Where(p => p.Gender == accountParam.Gender);
            }
            return list.ToList();
        }

        public void UpdateStatusAccount(int id, int status) {
            var user = _userDAO.GetIgnoreStatus(id);
            if ( user == null ) {
                throw new Exception("404: Không tìm thấy tài khoản");
            }
            user.Status = status;
            _userDAO.Update(user);
        }
    }
}

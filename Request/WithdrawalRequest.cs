using Request.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Request {
    public class WithdrawalRequest : IValidatableObject{
        [Required(ErrorMessage = "Không được để trống số tiền rút")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "Không được để trống tên ngân hàng")]
        [MaxLength(20, ErrorMessage = "Tên ngân hàng tối đa 20 ký tự")]
        public string Bank { get; set; }
        [Required(ErrorMessage = "Không được để trống số tài khoản ngân hàng")]
        public string BankNumber { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            if (Amount < 50000) {
                yield return new ValidationResult(
                    $"Số tiền rút phải lớn hơn 50.000 VNĐ",
                    new[] { nameof(Amount) });
            }
            var bankData = GetBankData();
            if (!bankData.Contains(Bank)) {
                yield return new ValidationResult(
                    $"Ngân hàng: \"{Bank}\" không nằm trong danh sách ngân hàng chúng tôi hỗ trợ",
                    new[] { nameof(Bank) });
            }
            
            if (!BankNumber.All(c => c >= '0' && c <= '9')) {
                yield return new ValidationResult(
                    $"Tài khoản ngân hàng của bạn: \"{BankNumber}\" chỉ được chứa số",
                    new[] { nameof(Bank) });
            }
        }

        public string[] GetBankData() {

            //JsonSerializerOptions _options = new() {
            //    PropertyNameCaseInsensitive = true
            //};
            //string path = System.IO.Directory.GetParent(Environment.CurrentDirectory).ToString() + "/Request/Data/Bank.json";
            ////var path = "./../Request/Data/Bank.json";
            //using FileStream json = File.OpenRead(path);
            return Data.Bank.GetBankList();
        }
    }

    public class UpdateWithdrawRequest {
        [Required]
        public int WithdrawalId { get; set; }
        [Required]
        [Range(2,3)]
        public int Status { get; set; }
    }
}

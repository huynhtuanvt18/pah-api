using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Request
{
    public class OrderRequest
    {
    }

    public class ConfirmOrderRequest
    {
        [Required]
        public int Status { get; set; }
        [Required]
        public string message { get; set; }
    }

    public class CheckoutRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Danh sách đơn hàng không được để trống")]
        public List<CheckoutOrder> Order { get; set; }
        [Required(ErrorMessage = "Tổng giá trị đơn hàng không được để trống")]
        public decimal Total { get; set; }
        [Required(ErrorMessage = "Phương thức thanh toán không được để trống")]
        public int PaymentType { get; set; }
        [Required(ErrorMessage = "Không được để trống ID địa chỉ giao hàng")]
        public int AddressId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            decimal totalFromOrders = 0m;
            foreach (var order in Order)
            {
                totalFromOrders += order.Total;
            }
            if (totalFromOrders != Total)
            {
                yield return new ValidationResult(
                    $"Total calculated from Orders is {totalFromOrders} which is different from Total sent is {Total}",
                    new[] { nameof(Total) });
            }
        }
    }

    public class CheckoutOrder : IValidatableObject
    {
        [Required(ErrorMessage = "ID người bán không được để trống")]
        public int SellerId { get; set; }
        [Required(ErrorMessage = "Danh sách sản phẩm đơn hàng không được để trống")]
        public List<CheckoutProduct> Products { get; set; }
        [Required(ErrorMessage = "Tổng giá trị đơn hàng không được để trống")]
        public decimal Total { get; set; }
        [Required(ErrorMessage = "Phí vận chuyển không được để trống")]
        public decimal ShippingCost { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            decimal totalFromProducts = 0m;
            foreach (var product in Products)
            {
                totalFromProducts += product.Price * product.Amount;
            }
            if (totalFromProducts != Total)
            {
                yield return new ValidationResult(
                    $"Tổng đơn hàng là {totalFromProducts} không khớp với tổng đơn hàng nhận được {Total}",
                    new[] { nameof(Total) });
            }
        }
    }

    public class CheckoutProduct
    {
        [Required(ErrorMessage = "ID sản phẩm không được để trống")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Giá sản phẩm không được để trống")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Số lượng sản phẩm không được để trống")]
        public int Amount { get; set; }
    }
}

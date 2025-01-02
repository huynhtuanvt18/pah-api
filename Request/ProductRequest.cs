using System.ComponentModel.DataAnnotations;

namespace Request
{
    public class ProductRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Không được để trống danh mục")]
        public int? CategoryId { get; set; }
        [Required(ErrorMessage = "Không được để trống vật liệu")]
        public int? MaterialId { get; set; }
        public int? SellerId { get; set; }
        [Required(ErrorMessage = "Không được để trống tên sản phẩm")]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [Required(ErrorMessage = "Không được để trống giá cả")]
        [Range(1000, Double.MaxValue, ErrorMessage = "Giá cả ít nhất phải là 1.000 VNĐ")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Không được để trống kích thước")]
        [RegularExpression(@"^\d+x\d+x\d+$", ErrorMessage = "Kích thước không hợp lệ")]
        public string? Dimension { get; set; }
        [Required(ErrorMessage = "Không được để trống khối lượng")]
        [Range(1, 30000, ErrorMessage = "Khối lượng ít nhất phải là 1 gram và nhiều nhất là 30000 gram")]
        public decimal? Weight { get; set; }
        [Required(ErrorMessage = "Không được để trống xuất xứ")]
        public string? Origin { get; set; }
        [Required(ErrorMessage = "Không được để trống phương thức gói hàng")]
        public string? PackageMethod { get; set; }
        [Required(ErrorMessage = "Không được để trống nội dung hàng")]
        public string? PackageContent { get; set; }
        [Required(ErrorMessage = "Không được để trống tình trạng sản phẩm")]
        public int Condition { get; set; }
        [Required(ErrorMessage = "Không được để trống loại sản phẩm")]
        [Range(1, 2, ErrorMessage = "Sản phẩm phải là loại mua bán hoặc đấu giá")]
        public int Type { get; set; }
        public string Title { get; set; } = null!;
        public decimal Step { get; set; }
        public List<string> ImageUrlLists { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == 2)
            {
                if (string.IsNullOrEmpty(Title))
                {
                    yield return new ValidationResult("Cần bổ sung tựa đề đấu giá",
                    new[] { nameof(Title) });
                }
                if (Step < 50000)
                {
                    yield return new ValidationResult("Bước giá cần lớn hơn 50.000 VNĐ",
                    new[] { nameof(Step) });
                }
            }

            decimal[] sizes = Dimension.Split('x').Select(decimal.Parse).ToArray();
            if (sizes[0] <= 0 || sizes[0] > 150)
            {
                yield return new ValidationResult("Chiều dài cần phải lớn hơn 0 cm và nhỏ hơn hoặc bằng 150 cm",
                    new[] { nameof(Dimension) });
            }
            if (sizes[1] <= 0 || sizes[1] > 150)
            {
                yield return new ValidationResult("Chiều dài cần phải lớn hơn 0 cm và nhỏ hơn hoặc bằng 150 cm",
                    new[] { nameof(Dimension) });
            }
            if (sizes[2] <= 0 || sizes[2] > 150)
            {
                yield return new ValidationResult("Chiều dài cần phải lớn hơn 0 cm và nhỏ hơn hoặc bằng 150 cm",
                    new[] { nameof(Dimension) });
            }
        }
    }
}

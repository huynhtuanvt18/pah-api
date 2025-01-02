using System.ComponentModel.DataAnnotations;

namespace Request.Param {
    //public class OrderParam : IValidatableObject {
    public class OrderParam
    {
        public List<int> Status { get; set; } = new List<int>();

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
        //    switch (Status) {
        //        case 0:
        //        case 1:
        //        case 2:
        //        case 3:
        //        case 4:
        //        case 5:
        //        case 6:
        //        case 10:
        //        case 11:
        //        case 12:
        //            break;
        //        default:
        //            Status = 0;
        //            yield return new ValidationResult(
        //                $"Status not follow system",
        //                new[] { nameof(Status)
        //            });
        //            break;
        //    }
        //}
    }
}

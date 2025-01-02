namespace Respon.UserRes {
    public class UserResponse {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? ProfilePicture { get; set; }
        public int? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public int Role { get; set; }
        public int Status { get; set; }
    }
}

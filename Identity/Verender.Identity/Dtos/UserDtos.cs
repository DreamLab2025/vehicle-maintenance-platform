namespace Verender.Identity.Dtos
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; }
        public bool IsPhoneNumberConfirmed { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }
}

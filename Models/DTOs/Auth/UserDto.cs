namespace AttandanceSyncApp.Models.DTOs.Auth
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string ProfilePicture { get; set; }
        public string SessionToken { get; set; }
        public bool IsActive { get; set; }
    }
}

namespace Core.Dtos.Requests
{
    public class CreateUserDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "Viewer";
        public List<string> Permissions { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}

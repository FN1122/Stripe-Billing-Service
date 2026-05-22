namespace Core.Dtos.Requests
{
    public class UpdateUserDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public bool? IsActive { get; set; }
        public List<string> Permissions { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}

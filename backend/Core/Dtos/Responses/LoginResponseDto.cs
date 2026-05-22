namespace Core.Dtos.Responses
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserResponseDto User { get; set; }
    }
}

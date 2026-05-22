namespace Core.Dtos.Responses
{
    public class TenantResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string PublicKey { get; set; }
        public string Plan { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TotalCustomers { get; set; }
        public List<string> Features { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

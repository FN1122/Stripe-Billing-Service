namespace Core.Dtos.Requests
{
    public class UpdateCouponDto
    {
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public int? MaxRedemptions { get; set; }
        public DateTime? RedeemBy { get; set; }
        public string? Metadata { get; set; }
    }
}

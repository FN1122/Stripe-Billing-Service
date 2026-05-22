namespace Core.Dtos.Requests
{
    public class UpdateCustomerDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Currency { get; set; }
        public string BillingAddress { get; set; }
        public string TaxId { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }
}

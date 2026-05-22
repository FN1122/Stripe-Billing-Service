namespace Core.Dtos.Requests
{
    public class UpdatePlanDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Features { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; }
    }
}

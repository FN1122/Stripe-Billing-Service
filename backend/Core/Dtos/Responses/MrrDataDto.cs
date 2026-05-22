namespace Core.Dtos.Responses
{
    public class MrrDataDto
    {
        public decimal CurrentMrr { get; set; }
        public decimal PreviousMrr { get; set; }
        public decimal MrrGrowth { get; set; }
        public decimal NewMrr { get; set; }
        public decimal ExpansionMrr { get; set; }
        public decimal ContractionMrr { get; set; }
        public decimal ChurnedMrr { get; set; }
        public decimal NetNewMrr { get; set; }
        public List<MrrHistoryPoint> MrrHistory { get; set; } = new();
    }

    public class MrrHistoryPoint
    {
        public string Month { get; set; }
        public decimal Mrr { get; set; }
    }
}

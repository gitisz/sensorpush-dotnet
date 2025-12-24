public class BackfillRequest
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; } = DateTime.UtcNow;
    public List<string>? SensorIds { get; set; }  // Optional filter
}
namespace BasketStats.Application.DTOs;

public class EventDto
{
    public string Id { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int PeriodNumber { get; set; }
    public int PeriodTimestamp { get; set; }
    public int? Points { get; set; }
    public decimal? CoordinatesX { get; set; }
    public decimal? CoordinatesY { get; set; }
    public bool? Made { get; set; }
    public string? FoulType { get; set; }
    public string? PlayerFouledId { get; set; }
    public bool? Flagrant { get; set; }
    public string? PlayerOutId { get; set; }
}

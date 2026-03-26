namespace BasketStats.Application.DTOs;

public record PeriodDto(int Number, DateTime StartTime, DateTime? EndTime, int? DurationSeconds);

namespace BasketStats.Application.Mapping;

using BasketStats.Application.DTOs;
using BasketStats.Domain.Entities;

public static class MatchMapper
{
    public static MatchDto ToDto(Match match)
    {
        var homeScore = CalculateScore(match, match.HomeTeamId);
        var awayScore = CalculateScore(match, match.AwayTeamId);

        return new MatchDto
        {
            Id = match.Id.Value,
            HomeTeamId = match.HomeTeamId,
            AwayTeamId = match.AwayTeamId,
            Status = match.Status.ToString(),
            CreatedAt = match.CreatedAt,
            StartedAt = match.StartedAt,
            FinishedAt = match.FinishedAt,
            Events = match.Events.Select(ToDto).ToList(),
            HomeScore = homeScore,
            AwayScore = awayScore
        };
    }

    public static EventDto ToDto(Event @event)
    {
        var dto = new EventDto
        {
            Id = @event.Id.Value,
            TeamId = @event.TeamId,
            PlayerId = @event.PlayerId,
            Type = @event.Type.ToString(),
            Timestamp = @event.Timestamp,
            PeriodNumber = (int)@event.PeriodNumber,
            PeriodTimestamp = @event.PeriodTimestamp
        };

        switch (@event)
        {
            case ScoreEvent score:
                dto.Points = score.Points;
                dto.CoordinatesX = score.Coordinates.X;
                dto.CoordinatesY = score.Coordinates.Y;
                break;
            case MissedShotEvent missed:
                dto.CoordinatesX = missed.Coordinates.X;
                dto.CoordinatesY = missed.Coordinates.Y;
                break;
            case FreeThrowEvent freeThrow:
                dto.Made = freeThrow.Made;
                dto.FoulType = freeThrow.FoulType.ToString();
                break;
            case FoulEvent foul:
                dto.FoulType = foul.FoulType.ToString();
                dto.PlayerFouledId = foul.PlayerFouledId;
                dto.Flagrant = foul.Flagrant;
                break;
            case SubstitutionEvent substitution:
                dto.PlayerOutId = substitution.PlayerOutId;
                break;
        }

        return dto;
    }

    private static int CalculateScore(Match match, string teamId)
    {
        var scorePoints = match.Events
            .OfType<ScoreEvent>()
            .Where(e => e.TeamId == teamId)
            .Sum(e => e.Points);

        var freeThrowPoints = match.Events
            .OfType<FreeThrowEvent>()
            .Where(e => e.TeamId == teamId && e.Made)
            .Count();

        return scorePoints + freeThrowPoints;
    }
}

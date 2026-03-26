using System.Reflection;
using BasketStats.Domain.Entities;
using BasketStats.Domain.Enums;
using BasketStats.Domain.ValueObjects;
using BasketStats.Infrastructure.Models;

namespace BasketStats.Infrastructure.Mapping;

public static class MatchFirestoreMapper
{
    public static MatchDocument ToDocument(Match match)
    {
        return new MatchDocument
        {
            Id = match.Id.Value,
            HomeTeamId = match.HomeTeamId,
            AwayTeamId = match.AwayTeamId,
            Status = (int)match.Status,
            CreatedAt = match.CreatedAt,
            StartedAt = match.StartedAt,
            FinishedAt = match.FinishedAt,
            Events = match.Events.Select(ToEventDocument).ToList(),
            Periods = match.Periods.Select(ToPeriodDocument).ToList(),
            HomePlayers = match.HomePlayers.Select(p => new PlayerDocument { Id = p.Id, Name = p.Name, Number = p.Number }).ToList(),
            AwayPlayers = match.AwayPlayers.Select(p => new PlayerDocument { Id = p.Id, Name = p.Name, Number = p.Number }).ToList()
        };
    }

    public static Match ToDomain(MatchDocument doc)
    {
        var match = Match.Create(doc.HomeTeamId, doc.AwayTeamId);

        typeof(Match).GetProperty(nameof(Match.Id))!
            .SetValue(match, new MatchId(doc.Id));

        typeof(Match).GetProperty(nameof(Match.Status))!
            .SetValue(match, (MatchStatus)doc.Status);

        typeof(Match).GetProperty(nameof(Match.CreatedAt))!
            .SetValue(match, doc.CreatedAt);

        if (doc.StartedAt.HasValue)
            typeof(Match).GetProperty(nameof(Match.StartedAt))!
                .SetValue(match, doc.StartedAt);

        if (doc.FinishedAt.HasValue)
            typeof(Match).GetProperty(nameof(Match.FinishedAt))!
                .SetValue(match, doc.FinishedAt);

        var eventsField = typeof(Match).GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var eventsList = (List<Event>)eventsField.GetValue(match)!;
        foreach (var eventDoc in doc.Events)
            eventsList.Add(ToEventDomain(eventDoc));

        var periodsField = typeof(Match).GetField("_periods", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var periodsList = (List<Period>)periodsField.GetValue(match)!;
        periodsList.Clear();
        foreach (var periodDoc in doc.Periods)
            periodsList.Add(ToPeriodDomain(periodDoc));

        var homePlayersField = typeof(Match).GetField("_homePlayers", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var homePlayersList = (List<PlayerInfo>)homePlayersField.GetValue(match)!;
        foreach (var pd in doc.HomePlayers)
            homePlayersList.Add(PlayerInfo.Restore(pd.Id, pd.Name, pd.Number));

        var awayPlayersField = typeof(Match).GetField("_awayPlayers", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var awayPlayersList = (List<PlayerInfo>)awayPlayersField.GetValue(match)!;
        foreach (var pd in doc.AwayPlayers)
            awayPlayersList.Add(PlayerInfo.Restore(pd.Id, pd.Name, pd.Number));

        return match;
    }

    public static EventDocument ToEventDocument(Event @event)
    {
        var doc = new EventDocument
        {
            Id = @event.Id.Value,
            TeamId = @event.TeamId,
            PlayerId = @event.PlayerId,
            Type = (int)@event.Type,
            Timestamp = @event.Timestamp,
            PeriodNumber = (int)@event.PeriodNumber,
            PeriodTimestamp = @event.PeriodTimestamp
        };

        switch (@event)
        {
            case ScoreEvent score:
                doc.Points = score.Points;
                doc.CoordinatesX = (double)score.Coordinates.X;
                doc.CoordinatesY = (double)score.Coordinates.Y;
                break;
            case MissedShotEvent missed:
                doc.CoordinatesX = (double)missed.Coordinates.X;
                doc.CoordinatesY = (double)missed.Coordinates.Y;
                break;
            case FoulEvent foul:
                doc.FoulType = (int)foul.FoulType;
                doc.PlayerFouledId = foul.PlayerFouledId;
                doc.Flagrant = foul.Flagrant;
                break;
            case SubstitutionEvent substitution:
                doc.PlayerOutId = substitution.PlayerOutId;
                break;
            case TurnoverEvent:
                break;
        }

        return doc;
    }

    public static Event ToEventDomain(EventDocument doc)
    {
        var type = (EventType)doc.Type;
        Event @event = type switch
        {
            EventType.Score => new ScoreEvent(
                doc.TeamId, doc.PlayerId, doc.Points!.Value,
                new Coordinates((decimal)doc.CoordinatesX!.Value, (decimal)doc.CoordinatesY!.Value),
                (PeriodNumber)doc.PeriodNumber, doc.PeriodTimestamp),
            EventType.MissedShot => new MissedShotEvent(
                doc.TeamId, doc.PlayerId,
                new Coordinates((decimal)doc.CoordinatesX!.Value, (decimal)doc.CoordinatesY!.Value),
                (PeriodNumber)doc.PeriodNumber, doc.PeriodTimestamp),
            EventType.Foul => new FoulEvent(
                doc.TeamId, doc.PlayerId, (FoulType)doc.FoulType!.Value,
                doc.PlayerFouledId!, doc.Flagrant!.Value,
                (PeriodNumber)doc.PeriodNumber, doc.PeriodTimestamp),
            EventType.Substitution => new SubstitutionEvent(
                doc.TeamId, doc.PlayerId, doc.PlayerOutId!,
                (PeriodNumber)doc.PeriodNumber, doc.PeriodTimestamp),
            EventType.Turnover => new TurnoverEvent(
                doc.TeamId, doc.PlayerId,
                (PeriodNumber)doc.PeriodNumber, doc.PeriodTimestamp),
            _ => throw new InvalidOperationException($"Unknown event type: {type}")
        };

        typeof(Event).GetProperty(nameof(Event.Id))!.SetValue(@event, new EventId(doc.Id));
        typeof(Event).GetProperty(nameof(Event.Timestamp))!.SetValue(@event, doc.Timestamp);

        return @event;
    }

    public static PeriodDocument ToPeriodDocument(Period period)
    {
        return new PeriodDocument
        {
            Number = (int)period.Number,
            StartTime = period.StartTime,
            EndTime = period.EndTime
        };
    }

    public static Period ToPeriodDomain(PeriodDocument doc)
    {
        var period = new Period((PeriodNumber)doc.Number, doc.StartTime);
        if (doc.EndTime.HasValue)
            period.End(doc.EndTime.Value);
        return period;
    }
}

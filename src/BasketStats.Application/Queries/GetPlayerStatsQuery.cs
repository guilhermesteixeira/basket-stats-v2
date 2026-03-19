namespace BasketStats.Application.Queries;

using MediatR;
using BasketStats.Application.DTOs;

public record GetPlayerStatsQuery(string MatchId, string PlayerId) : IRequest<PlayerStatsDto?>;

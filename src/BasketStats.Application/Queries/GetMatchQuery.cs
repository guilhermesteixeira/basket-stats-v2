namespace BasketStats.Application.Queries;

using MediatR;
using BasketStats.Application.DTOs;

public record GetMatchQuery(string MatchId) : IRequest<MatchDto?>;

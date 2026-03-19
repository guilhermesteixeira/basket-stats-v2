namespace BasketStats.Application.Queries;

using MediatR;
using BasketStats.Application.DTOs;
using BasketStats.Domain.Enums;

public record ListMatchesQuery(string? TeamId = null, MatchStatus? Status = null) : IRequest<List<MatchDto>>;

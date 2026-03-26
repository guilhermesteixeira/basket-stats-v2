namespace BasketStats.Application.Queries;

using MediatR;
using BasketStats.Application.DTOs;

public record ListTeamsQuery() : IRequest<List<TeamDto>>;

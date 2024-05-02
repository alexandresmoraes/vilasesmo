﻿using Common.WebAPI.Results;
using MediatR;
using System.Text.Json.Serialization;
using Vendas.API.Application.Responses;

namespace Vendas.API.Application.Commands
{
  public record CriarVendaCommand : IRequest<Result<CriarVendaCommandResponse>>
  {
    [JsonIgnore]
    public string? UserId { get; private set; }
    public string? CompradorNome { get; init; }
    public string? CompradorFotoUrl { get; init; }
    public int? TipoPagamento { get; init; } = null!;

    public void SetUserId(string? userId) => UserId = userId;
  }
}
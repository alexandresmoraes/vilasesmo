﻿using Common.WebAPI.Results;
using MediatR;

namespace Vendas.API.Application.Commands
{
  public record CancelarPagamentoCommand : IRequest<Result>
  {
    public long PagamentoId { get; private init; }

    public CancelarPagamentoCommand(long pagamentoId)
    {
      PagamentoId = pagamentoId;
    }
  }
}
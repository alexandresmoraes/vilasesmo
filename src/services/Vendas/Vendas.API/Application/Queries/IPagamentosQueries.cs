﻿using Common.WebAPI.Results;

namespace Vendas.API.Application.Queries
{
  public interface IPagamentosQueries
  {
    Task<PagamentoDetalheDto> GetPagamentoPendentes(string userId, CancellationToken cancellationToken = default);
    Task<PagedResult<PagamentosDto>> GetPagamentos(PagamentosQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<PagamentosDto>> GetMeusPagamentos(MeusPagamentosQuery query, CancellationToken cancellationToken = default);
  }
}
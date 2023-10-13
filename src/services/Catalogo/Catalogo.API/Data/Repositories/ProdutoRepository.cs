﻿using Catalogo.API.Data.Dto;
using Catalogo.API.Data.Entities;
using Catalogo.API.Data.Queries;
using Common.WebAPI.MongoDb;
using Common.WebAPI.Results;
using Common.WebAPI.Utils;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace Catalogo.API.Data.Repositories
{
  public class ProdutoRepository : MongoService<Produto>, IProdutoRepository
  {
    private readonly FavoritoItemRepository _favoriteItemRepository;

    public ProdutoRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> opt, FavoritoItemRepository favoriteItemRepository)
     : base(mongoClient, opt, "produtos")
    {
      Collection.Indexes.CreateOne(new CreateIndexModel<Produto>(
        Builders<Produto>.IndexKeys.Ascending(_ => _.Nome),
        new CreateIndexOptions { Unique = true }
      ));

      Collection.Indexes.CreateOne(new CreateIndexModel<Produto>(
        Builders<Produto>.IndexKeys.Ascending(_ => _.CodigoBarras),
        new CreateIndexOptions { Unique = true }
      ));

      Collection.Indexes.CreateOne(new CreateIndexModel<Produto>(
        Builders<Produto>.IndexKeys.Descending(_ => _.Nome)
      ));

      Collection.Indexes.CreateOne(new CreateIndexModel<Produto>(
        Builders<Produto>.IndexKeys.Descending(_ => _.DataUltimaVenda)
      ));

      Collection.Indexes.CreateOne(new CreateIndexModel<Produto>(
        Builders<Produto>.IndexKeys.Descending(_ => _.DataCriacao)
      ));

      Collection.Indexes.CreateOne(new CreateIndexModel<Produto>(
        Builders<Produto>.IndexKeys.Ascending(_ => _.Rating)
      ));

      Collection.Indexes.CreateOne(new CreateIndexModel<Produto>(
        Builders<Produto>.IndexKeys.Descending(_ => _.Rating)
      ));

      Collection.Indexes.CreateOne(new CreateIndexModel<Produto>(
        Builders<Produto>.IndexKeys.Descending(_ => _.IsAtivo)
      ));

      _favoriteItemRepository = favoriteItemRepository;
    }

    public async Task CreateAsync(Produto produto)
      => await Collection.InsertOneAsync(produto);

    public async Task<bool> ExisteProdutoPorCodigoBarras(string codigoBarras, string? id)
    {
      var filtro = Builders<Produto>.Filter.Eq(p => p.CodigoBarras, codigoBarras);

      if (!string.IsNullOrEmpty(id))
      {
        filtro &= Builders<Produto>.Filter.Ne(p => p.Id, id);
      }

      var count = await Collection.CountDocumentsAsync(filtro);

      return count > 0;
    }

    public async Task<bool> ExisteProdutoPorNome(string nome, string? id)
    {
      var filtro = Builders<Produto>.Filter.Eq(p => p.Nome, nome);

      if (!string.IsNullOrEmpty(id))
      {
        filtro &= Builders<Produto>.Filter.Ne(p => p.Id, id);
      }

      var count = await Collection.CountDocumentsAsync(filtro);

      return count > 0;
    }

    public async Task<bool> ExisteProdutoPorId(string id)
    {
      var filtro = Builders<Produto>.Filter.Eq(p => p.Id, id);

      var count = await Collection.CountDocumentsAsync(filtro);

      return count > 0;
    }

    public async Task<Produto?> GetAsync(string id)
      => await Collection.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task<PagedResult<ProdutoDto>> GetProdutosAsync(ProdutoQuery produtoQuery)
    {
      var filtro = Builders<Produto>.Filter.Empty;

      if (!string.IsNullOrWhiteSpace(produtoQuery.nome))
      {
        filtro &= Builders<Produto>.Filter.Regex(p => p.Nome, new BsonRegularExpression(new Regex(produtoQuery.nome, RegexOptions.IgnoreCase)));
      }

      var start = (produtoQuery.page - 1) * produtoQuery.limit;

      var projections = Builders<Produto>.Projection
        .Expression(p => new ProdutoDto
        {
          Id = p.Id,
          Nome = p.Nome,
          Descricao = p.Descricao,
          ImageUrl = p.ImageUrl,
          Preco = p.Preco,
          UnidadeMedida = p.UnidadeMedida,
          EstoqueAlvo = p.EstoqueAlvo,
          Estoque = p.Estoque,
          Rating = p.Rating,
          RatingCount = p.RatingCount,
          IsAtivo = p.IsAtivo
        });

      var produtos = await Collection.Find(filtro)
        .SortByDescending(p => p.IsAtivo)
        .ThenBy(p => p.Nome)
        .Skip((produtoQuery.page - 1) * produtoQuery.limit)
        .Limit(produtoQuery.limit)
        .Project(projections)
        .ToListAsync();

      var count = await Collection.CountDocumentsAsync(filtro);

      return new PagedResult<ProdutoDto>(start, produtoQuery.limit, count, produtos);
    }

    public async Task UpdateAsync(Produto produto)
      => await Collection.ReplaceOneAsync(p => p.Id == produto.Id, produto);

    public async Task<ProdutoDetailDto?> GetProdutoDetailAsync(string userId, string produtoId)
    {
      var filtro = Builders<Notificacao>.Filter.Empty;

      var projections = Builders<Produto>.Projection
        .Expression(p => new ProdutoDetailDto
        {
          Id = p.Id,
          ImageUrl = p.ImageUrl,
          Nome = p.Nome,
          Descricao = p.Descricao,
          Preco = p.Preco,
          UnidadeMedida = p.UnidadeMedida,
          CodigoBarras = p.CodigoBarras,
          Rating = p.Rating,
          RatingCount = p.RatingCount,
          Estoque = p.Estoque,
          IsAtivo = p.IsAtivo
        });

      var produtoDetailDto = await Collection
        .Find(p => p.Id == produtoId)
        .Project(projections)
        .SingleOrDefaultAsync();

      if (produtoDetailDto is not null)
      {
        produtoDetailDto.IsFavorito = _favoriteItemRepository
          .Collection.Find(f => f.ProdutoId == produtoId && f.UserId == userId).Any();
      }

      return produtoDetailDto;
    }

    public async Task<PagedResult<CatalogoDto>> GetProdutosFavoritosAsync(string userId, CatalogoQuery query)
    {
      var start = (query.page - 1) * query.limit;

      var lookupStage = new BsonDocument("$lookup",
        new BsonDocument
        {
          { "from", "produtos" },
          { "localField", "ProdutoId" },
          { "foreignField", "_id" },
          { "as", "produto" }
        });

      var unwindStage = new BsonDocument("$unwind", "$produto");

      var matchStage = new BsonDocument
      {
        {
          "$match",
          new BsonDocument
          {
            { "UserId", userId }
          }
       }
      };

      var projectStage = new BsonDocument("$project",
        new BsonDocument
        {
          { "_id", 0 },
          { "ProdutoId", "$ProdutoId" },
          { "Nome", "$produto.Nome" },
          { "Descricao", "$produto.Descricao" },
          { "UnidadeMedida", "$produto.UnidadeMedida" },
          { "ImageUrl", "$produto.ImageUrl" },
          { "Estoque", "$produto.Estoque" },
          { "Preco", "$produto.Preco" },
          { "Rating", "$produto.Rating" },
          { "RatingCount", "$produto.RatingCount" },
          { "IsAtivo", "$produto.IsAtivo" },
        });

      var skipStage = new BsonDocument("$skip", start);
      var limitStage = new BsonDocument("$limit", query.limit);

      var pipeline = new[] { lookupStage, unwindStage, matchStage, projectStage, skipStage, limitStage };
      var aggregation = await _favoriteItemRepository.Collection.Aggregate<CatalogoDto>(pipeline).ToListAsync();

      var count = await _favoriteItemRepository.Collection.CountDocumentsAsync(Builders<FavoritoItem>.Filter.Eq(_ => _.UserId, userId));

      return new PagedResult<CatalogoDto>(start, query.limit, count, aggregation);
    }

    public async Task<PagedResult<CatalogoDto>> GetProdutosMaisVendidosAsync(CatalogoQuery catalogoQuery)
    {
      var filtro = Builders<Produto>.Filter.Ne(p => p.QuantidadeVendida, 0);
      filtro &= Builders<Produto>.Filter.Eq(p => p.IsAtivo, true);

      var start = (catalogoQuery.page - 1) * catalogoQuery.limit;

      var projections = Builders<Produto>.Projection
        .Expression(p => new CatalogoDto
        {
          ProdutoId = p.Id,
          Nome = p.Nome,
          Descricao = p.Descricao,
          ImageUrl = p.ImageUrl,
          Preco = p.Preco,
          UnidadeMedida = p.UnidadeMedida,
          Estoque = p.Estoque,
          Rating = p.Rating,
          RatingCount = p.RatingCount,
          IsAtivo = p.IsAtivo
        });

      var catalogo = await Collection.Find(filtro)
        .SortByDescending(p => p.QuantidadeVendida)
        .Skip((catalogoQuery.page - 1) * catalogoQuery.limit)
        .Limit(catalogoQuery.limit)
        .Project(projections)
        .ToListAsync();

      var count = await Collection.CountDocumentsAsync(filtro);

      return new PagedResult<CatalogoDto>(start, catalogoQuery.limit, count, catalogo);
    }

    public async Task<PagedResult<CatalogoDto>> GetProdutosNovosAsync(CatalogoQuery catalogoQuery)
    {
      var filtro = Builders<Produto>.Filter.Eq(p => p.IsAtivo, true);

      var start = (catalogoQuery.page - 1) * catalogoQuery.limit;

      var projections = Builders<Produto>.Projection
        .Expression(p => new CatalogoDto
        {
          ProdutoId = p.Id,
          Nome = p.Nome,
          Descricao = p.Descricao,
          ImageUrl = p.ImageUrl,
          Preco = p.Preco,
          UnidadeMedida = p.UnidadeMedida,
          Estoque = p.Estoque,
          Rating = p.Rating,
          RatingCount = p.RatingCount,
          IsAtivo = p.IsAtivo
        });

      var catalogo = await Collection.Find(filtro)
        .SortByDescending(p => p.DataCriacao)
        .Skip((catalogoQuery.page - 1) * catalogoQuery.limit)
        .Limit(catalogoQuery.limit)
        .Project(projections)
        .ToListAsync();

      var count = await Collection.CountDocumentsAsync(filtro);

      return new PagedResult<CatalogoDto>(start, catalogoQuery.limit, count, catalogo);
    }

    public async Task<PagedResult<CatalogoDto>> GetProdutosUltimosVendidosAsync(CatalogoQuery catalogoQuery)
    {
      var filtro = Builders<Produto>.Filter.Ne(p => p.DataUltimaVenda, null);
      filtro &= Builders<Produto>.Filter.Eq(p => p.IsAtivo, true);

      var start = (catalogoQuery.page - 1) * catalogoQuery.limit;

      var projections = Builders<Produto>.Projection
        .Expression(p => new CatalogoDto
        {
          ProdutoId = p.Id,
          Nome = p.Nome,
          Descricao = p.Descricao,
          ImageUrl = p.ImageUrl,
          Preco = p.Preco,
          UnidadeMedida = p.UnidadeMedida,
          Estoque = p.Estoque,
          Rating = p.Rating,
          RatingCount = p.RatingCount,
          IsAtivo = p.IsAtivo
        });

      var catalogo = await Collection.Find(filtro)
        .SortByDescending(p => p.DataUltimaVenda)
        .Skip((catalogoQuery.page - 1) * catalogoQuery.limit)
        .Limit(catalogoQuery.limit)
        .Project(projections)
        .ToListAsync();

      var count = await Collection.CountDocumentsAsync(filtro);

      return new PagedResult<CatalogoDto>(start, catalogoQuery.limit, count, catalogo);
    }

    public async Task<PagedResult<CatalogoDto>> GetTodosProdutosAtivosAsync(string userId, CatalogoTodosQuery query)
    {
      var filtro = Builders<Produto>.Filter.Eq(p => p.IsAtivo, true);

      if (!query.inStock || !query.outOfStock)
      {
        if (query.inStock || !query.outOfStock)
          filtro &= Builders<Produto>.Filter.Gt(p => p.Estoque, 0);

        if (query.outOfStock || !query.inStock)
          filtro &= Builders<Produto>.Filter.Lt(p => p.Estoque, 1);
      }

      var start = (query.page - 1) * query.limit;

      var catalogoPipeline = new BsonDocument[]
      {
        new BsonDocument
        {
          {
            "$match", filtro.RenderToBsonDocument()
          }
        },
        new BsonDocument("$lookup", new BsonDocument
            {
              { "from", "favoritos" },
              { "let", new BsonDocument("produtoId", "$_id") },
              { "pipeline", new BsonArray
              {
                new BsonDocument("$match", new BsonDocument
                {
                  { "$expr", new BsonDocument("$and", new BsonArray
                    {
                      new BsonDocument("$eq", new BsonArray { "$ProdutoId", "$$produtoId" }),
                      new BsonDocument("$eq", new BsonArray { "$UserId", userId })
                    })
                  }
                })
            }
          },
          { "as", "Favoritos" }
        }),
        new BsonDocument("$project", new BsonDocument
        {
          { "_id", 0 },
          { "ProdutoId", "$_id" },
          { "Nome", 1 },
          { "Descricao", 1 },
          { "ImageUrl", 1 },
          { "Preco", 1 },
          { "UnidadeMedida", 1 },
          { "Estoque", 1 },
          { "Rating", 1 },
          { "RatingCount", 1 },
          { "IsAtivo", 1 },
          { "IsFavorito", new BsonDocument("$cond", new BsonArray
              {
                  new BsonDocument("$gt", new BsonArray { new BsonDocument("$size", "$Favoritos"), 0 }),
                  true,
                  false
              })
          }
        }),
        new BsonDocument("$sort", new BsonDocument(query.order switch
        {
          ECatalogoTodosQueryOrder.NameAsc => new BsonDocument("Nome", 1),
          ECatalogoTodosQueryOrder.NameDesc => new BsonDocument("Nome", -1),
          ECatalogoTodosQueryOrder.PriceHighToLow => new BsonDocument("Preco", -1),
          ECatalogoTodosQueryOrder.PriceLowToHigh => new BsonDocument("Preco", 1),
          _ => new BsonDocument("Nome", 1)
        })),
        new BsonDocument("$skip", start),
        new BsonDocument("$limit", query.limit)
      };

      var catalogo = await Collection.Aggregate<CatalogoDto>(catalogoPipeline).ToListAsync();

      var count = await Collection.CountDocumentsAsync(filtro);

      return new PagedResult<CatalogoDto>(start, query.limit, count, catalogo);
    }

    public async Task UpdateReservarEstoque(Dictionary<string, int> produtos)
    {
      var loteUpdate = new List<WriteModel<Produto>>();

      foreach (var produto in produtos)
      {
        var filter = Builders<Produto>.Filter.Eq(p => p.Id, produto.Key);
        var update = Builders<Produto>.Update.Inc(p => p.Estoque, -produto.Value);
        loteUpdate.Add(new UpdateOneModel<Produto>(filter, update));
      }

      _ = await Collection.BulkWriteAsync(loteUpdate);
    }
  }
}
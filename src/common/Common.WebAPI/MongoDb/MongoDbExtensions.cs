﻿using Common.WebAPI.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Common.WebAPI.MongoDb
{
  public static class MongoDbExtensions
  {
    public static void UseMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
      services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

      services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

      services.AddSingleton<IMongoClient>(serviceProvider
        => new MongoClient(configuration.GetConnectionString("Default")));
    }

    public static IServiceCollection AddUnitOfWorkMongo(this IServiceCollection services)
    {
      services.AddScoped<IUnitOfWork, UnitOfWork>();

      services.AddMvc(opt =>
      {
        opt.Filters.Add<UnitOfWorkAttribute>();
        opt.Filters.Add<UnitOfWorkExceptionAttribute>();
      });

      return services;
    }

    public static IHealthChecksBuilder AddMongoHealthCheck(this IHealthChecksBuilder checkBuilder, IConfiguration configuration)
    {
      var connectionString = configuration.GetConnectionString("Default");

      if (string.IsNullOrEmpty(connectionString))
        throw new ArgumentException("Default connectionString is empty");

      return checkBuilder.AddMongoDb(connectionString, name: "mongodb", tags: new[] { "infra" });
    }
  }
}
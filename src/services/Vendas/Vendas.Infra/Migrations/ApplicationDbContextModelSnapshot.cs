﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Vendas.Infra.Data;

#nullable disable

namespace Vendas.Infra.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Vendas.Domain.Aggregates.Comprador", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("FotoUrl")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("foto_url");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("nome");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique()
                        .HasDatabaseName("comprador_userid_index");

                    b.ToTable("compradores", (string)null);
                });

            modelBuilder.Entity("Vendas.Domain.Aggregates.Pagamento.Pagamento", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("DataHora")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("datahora");

                    b.Property<int>("Tipo")
                        .HasColumnType("integer")
                        .HasColumnName("tipo");

                    b.Property<decimal>("Valor")
                        .HasColumnType("numeric")
                        .HasColumnName("valor");

                    b.Property<long>("comprador_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("comprador_id");

                    b.ToTable("pagamentos", (string)null);
                });

            modelBuilder.Entity("Vendas.Domain.Aggregates.Venda", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("DataHora")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("datahora");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<decimal>("Total")
                        .HasColumnType("numeric")
                        .HasColumnName("total");

                    b.Property<long>("comprador_id")
                        .HasColumnType("bigint");

                    b.Property<long?>("pagamento_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("comprador_id");

                    b.HasIndex("pagamento_id");

                    b.ToTable("vendas", (string)null);
                });

            modelBuilder.Entity("Vendas.Domain.Aggregates.VendaItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("descricao");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("image_url");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("nome");

                    b.Property<decimal>("Preco")
                        .HasColumnType("numeric")
                        .HasColumnName("preco");

                    b.Property<string>("ProdutoId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("produto_id");

                    b.Property<int>("Quantidade")
                        .HasColumnType("integer")
                        .HasColumnName("quantidade");

                    b.Property<string>("UnidadeMedida")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("unidade_medida");

                    b.Property<long>("VendaId")
                        .HasColumnType("bigint")
                        .HasColumnName("venda_id");

                    b.HasKey("Id");

                    b.HasIndex("VendaId");

                    b.ToTable("venda_itens", (string)null);
                });

            modelBuilder.Entity("Vendas.Domain.Aggregates.Pagamento.Pagamento", b =>
                {
                    b.HasOne("Vendas.Domain.Aggregates.Comprador", "Comprador")
                        .WithMany()
                        .HasForeignKey("comprador_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Comprador");
                });

            modelBuilder.Entity("Vendas.Domain.Aggregates.Venda", b =>
                {
                    b.HasOne("Vendas.Domain.Aggregates.Comprador", "Comprador")
                        .WithMany()
                        .HasForeignKey("comprador_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Vendas.Domain.Aggregates.Pagamento.Pagamento", null)
                        .WithMany("Vendas")
                        .HasForeignKey("pagamento_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Comprador");
                });

            modelBuilder.Entity("Vendas.Domain.Aggregates.VendaItem", b =>
                {
                    b.HasOne("Vendas.Domain.Aggregates.Venda", "Venda")
                        .WithMany("VendaItens")
                        .HasForeignKey("VendaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Venda");
                });

            modelBuilder.Entity("Vendas.Domain.Aggregates.Pagamento.Pagamento", b =>
                {
                    b.Navigation("Vendas");
                });

            modelBuilder.Entity("Vendas.Domain.Aggregates.Venda", b =>
                {
                    b.Navigation("VendaItens");
                });
#pragma warning restore 612, 618
        }
    }
}

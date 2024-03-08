﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Compras.Infra.Migrations
{
    public partial class Initial_Create : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compras",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    datahora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    user_email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    user_foto_url = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compra_itens",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    produto_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    estoque_atual = table.Column<int>(type: "integer", nullable: false),
                    preco_pago = table.Column<decimal>(type: "numeric", nullable: false),
                    preco_sugerido = table.Column<decimal>(type: "numeric", nullable: false),
                    preco_medio_sugerido = table.Column<bool>(type: "boolean", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    unidade_medida = table.Column<string>(type: "text", nullable: false),
                    compra_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compra_itens", x => x.id);
                    table.ForeignKey(
                        name: "FK_compra_itens_compras_compra_id",
                        column: x => x.compra_id,
                        principalTable: "compras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compra_itens_compra_id",
                table: "compra_itens",
                column: "compra_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compra_itens");

            migrationBuilder.DropTable(
                name: "compras");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Posto.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "postos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    razao_social = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    bandeira = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    localizacao = table.Column<Point>(type: "geography (point, 4326)", nullable: true),
                    endereco_bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    endereco_cep = table.Column<string>(type: "character(8)", fixedLength: true, maxLength: 8, nullable: true),
                    endereco_complemento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    endereco_logradouro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    endereco_municipio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    endereco_numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    endereco_uf = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_postos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "precos_combustivel",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    posto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    combustivel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    data_coleta = table.Column<DateOnly>(type: "date", nullable: false),
                    fonte = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_precos_combustivel", x => x.id);
                    table.CheckConstraint("ck_precos_combustivel_valor_positivo", "valor > 0");
                    table.ForeignKey(
                        name: "fk_precos_combustivel_postos_posto_id",
                        column: x => x.posto_id,
                        principalTable: "postos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_postos_cnpj",
                table: "postos",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_postos_localizacao",
                table: "postos",
                column: "localizacao")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_precos_combustivel_combustivel_data_coleta",
                table: "precos_combustivel",
                columns: new[] { "combustivel", "data_coleta" });

            migrationBuilder.CreateIndex(
                name: "ix_precos_combustivel_posto_id_combustivel_data_coleta_fonte",
                table: "precos_combustivel",
                columns: new[] { "posto_id", "combustivel", "data_coleta", "fonte" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "precos_combustivel");

            migrationBuilder.DropTable(
                name: "postos");
        }
    }
}

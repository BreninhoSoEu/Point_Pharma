using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Point_Pharma.Data.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarModeloProdutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecoAntigo = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    QuantidadeEstoque = table.Column<int>(type: "int", nullable: false),
                    FarmaciaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produtos_Farmacias_FarmaciaId",
                        column: x => x.FarmaciaId,
                        principalTable: "Farmacias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_FarmaciaId",
                table: "Produtos",
                column: "FarmaciaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Produtos");
        }
    }
}

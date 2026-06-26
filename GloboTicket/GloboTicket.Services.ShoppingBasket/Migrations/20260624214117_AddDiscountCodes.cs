using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GloboTicket.Services.ShoppingBasket.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscountCode",
                table: "Baskets",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DiscountCodes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountCodes", x => x.Code);
                });

            migrationBuilder.InsertData(
                table: "DiscountCodes",
                columns: new[] { "Code", "IsActive", "Type", "Value" },
                values: new object[,]
                {
                    { "FIVEOFF", true, 1, 5 },
                    { "SUMMER10", true, 0, 10 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscountCodes");

            migrationBuilder.DropColumn(
                name: "DiscountCode",
                table: "Baskets");
        }
    }
}

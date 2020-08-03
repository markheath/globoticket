using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GloboTicket.Services.EventCatalog.Migrations
{
    public partial class Tickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    TicketId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Price = table.Column<int>(nullable: false),
                    EventId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_Tickets_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea317"),
                column: "Date",
                value: new DateTime(2021, 2, 3, 12, 35, 47, 429, DateTimeKind.Local).AddTicks(6380));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea318"),
                column: "Date",
                value: new DateTime(2021, 4, 3, 12, 35, 47, 431, DateTimeKind.Local).AddTicks(9492));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea319"),
                column: "Date",
                value: new DateTime(2021, 5, 3, 12, 35, 47, 431, DateTimeKind.Local).AddTicks(9372));

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "TicketId", "EventId", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31a"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea317"), "Standard", 65 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31b"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea317"), "Premium", 95 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31c"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea319"), "Standard", 85 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31d"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea319"), "Premium", 110 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31e"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea318"), "Standard", 135 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31f"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea318"), "Premium", 190 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EventId",
                table: "Tickets",
                column: "EventId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea317"),
                column: "Date",
                value: new DateTime(2021, 1, 11, 16, 22, 42, 250, DateTimeKind.Local).AddTicks(4932));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea318"),
                column: "Date",
                value: new DateTime(2021, 3, 11, 16, 22, 42, 252, DateTimeKind.Local).AddTicks(5838));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea319"),
                column: "Date",
                value: new DateTime(2021, 4, 11, 16, 22, 42, 252, DateTimeKind.Local).AddTicks(5753));
        }
    }
}

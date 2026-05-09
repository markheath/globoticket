using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GloboTicket.Services.EventCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketsAvailable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TicketsAvailable",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea317"),
                column: "TicketsAvailable",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea318"),
                column: "TicketsAvailable",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea319"),
                column: "TicketsAvailable",
                value: 100);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea320"),
                column: "TicketsAvailable",
                value: 100);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea321"),
                column: "TicketsAvailable",
                value: 100);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea322"),
                column: "TicketsAvailable",
                value: 100);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea323"),
                column: "TicketsAvailable",
                value: 7);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea324"),
                column: "TicketsAvailable",
                value: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketsAvailable",
                table: "Events");
        }
    }
}

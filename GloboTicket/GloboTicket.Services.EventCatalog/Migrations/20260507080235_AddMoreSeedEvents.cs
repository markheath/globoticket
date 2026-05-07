using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GloboTicket.Services.EventCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreSeedEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "EventId", "Artist", "CategoryId", "Date", "Description", "ImageUrl", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea320"), "Aisha Patel", new Guid("cfb88e29-4744-48c0-94fa-b25b92dea314"), new DateTime(2027, 3, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Aisha Patel returns to the city stage for one night only, blending classical violin with the rhythms of her South-Asian heritage. A warm, intimate evening that has sold out venues from London to Singapore.", "/img/aisha.jpg", "An Evening with Aisha Patel", 70 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea321"), "Maya Okafor", new Guid("cfb88e29-4744-48c0-94fa-b25b92dea314"), new DateTime(2027, 5, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Three-time Grammy nominee Maya Okafor brings her signature blend of jazz, soul and contemporary R&B to the headline stage. Expect new material from her upcoming album alongside the songs you already love.", "/img/maya.jpg", "Midnight Sessions with Maya Okafor", 80 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea322"), "Priya Raman", new Guid("cfb88e29-4744-48c0-94fa-b25b92dea315"), new DateTime(2027, 7, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "A vibrant new musical from composer Priya Raman following four neighbours over one transformative summer. Critics have called it the freshest score Broadway has heard in years.", "/img/sunlight.jpg", "Sunlight Avenue", 120 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea323"), "Helena Marsh", new Guid("cfb88e29-4744-48c0-94fa-b25b92dea316"), new DateTime(2027, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Helena Marsh's quietly devastating two-hander has won this year's Olivier Award for Best New Play. A lighthouse, a long-kept secret, and a daughter returning home after twenty years away.", "/img/lighthouse.jpg", "The Lighthouse Keeper's Daughter", 55 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea324"), "Kenji Tanaka", new Guid("cfb88e29-4744-48c0-94fa-b25b92dea316"), new DateTime(2027, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kenji Tanaka's celebrated drama, translated into eleven languages, makes its long-awaited debut on the main stage. A correspondence between two strangers across a closed border, and what happens when the border finally opens.", "/img/letters.jpg", "Letters from the Border", 60 }
                });

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "TicketId", "EventId", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea325"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea320"), "Standard", 70 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea326"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea320"), "Premium", 110 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea327"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea321"), "Standard", 80 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea328"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea321"), "Premium", 130 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea329"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea322"), "Standard", 120 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea32a"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea322"), "Premium", 180 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea32b"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea323"), "Standard", 55 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea32c"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea323"), "Premium", 90 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea32d"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea324"), "Standard", 60 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea32e"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea324"), "Premium", 95 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea325"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea326"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea327"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea328"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea329"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea32a"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea32b"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea32c"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea32d"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "TicketId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea32e"));

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea320"));

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea321"));

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea322"));

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea323"));

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: new Guid("cfb88e29-4744-48c0-94fa-b25b92dea324"));
        }
    }
}

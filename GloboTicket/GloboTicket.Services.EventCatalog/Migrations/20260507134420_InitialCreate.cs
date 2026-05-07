using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GloboTicket.Services.EventCatalog.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    Artist = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_Events_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "Name" },
                values: new object[,]
                {
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea314"), "Concerts" },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea315"), "Musicals" },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea316"), "Plays" }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "EventId", "Artist", "CategoryId", "Date", "Description", "ImageUrl", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea317"), "John Egbert", new Guid("cfb88e29-4744-48c0-94fa-b25b92dea314"), new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Join John for his farwell tour across 15 continents. John really needs no introduction since he has already mesmerized the world with his banjo.", "/img/banjo.jpg", "John Egbert Live", 65 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea318"), "Nick Sailor", new Guid("cfb88e29-4744-48c0-94fa-b25b92dea315"), new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "The critics are over the moon and so will you after you've watched this sing and dance extravaganza written by Nick Sailor, the man from 'My dad and sister'.", "/img/musical.jpg", "To the Moon and Back", 135 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea319"), "Michael Johnson", new Guid("cfb88e29-4744-48c0-94fa-b25b92dea314"), new DateTime(2027, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Michael Johnson doesn't need an introduction. His 25 concert across the globe last year were seen by thousands. Can we add you to the list?", "/img/michael.jpg", "The State of Affairs: Michael Live!", 85 },
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
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31a"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea317"), "Standard", 65 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31b"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea317"), "Premium", 95 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31c"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea319"), "Standard", 85 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31d"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea319"), "Premium", 110 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31e"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea318"), "Standard", 135 },
                    { new Guid("cfb88e29-4744-48c0-94fa-b25b92dea31f"), new Guid("cfb88e29-4744-48c0-94fa-b25b92dea318"), "Premium", 190 },
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

            migrationBuilder.CreateIndex(
                name: "IX_Events_CategoryId",
                table: "Events",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EventId",
                table: "Tickets",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}

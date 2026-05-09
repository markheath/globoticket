using GloboTicket.Services.EventCatalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace GloboTicket.Services.EventCatalog.DbContexts
{
    public class EventCatalogDbContext : DbContext
    {
        public EventCatalogDbContext(DbContextOptions<EventCatalogDbContext> options) : base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Event.Date is a calendar date ("the show is on this day"), not
            // an instant in time. Npgsql maps DateTime to "timestamp with time
            // zone" by default, which rejects DateTimeKind.Unspecified values
            // — so we map this column to "timestamp without time zone" instead.
            modelBuilder.Entity<Event>()
                .Property(e => e.Date)
                .HasColumnType("timestamp without time zone");

            var concertGuid = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA314}");
            var musicalGuid = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA315}");
            var playGuid = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA316}");

            modelBuilder.Entity<Category>().HasData(new Category
            {
                CategoryId = concertGuid,
                Name = "Concerts"
            });
            modelBuilder.Entity<Category>().HasData(new Category
            {
                CategoryId = musicalGuid,
                Name = "Musicals"
            });
            modelBuilder.Entity<Category>().HasData(new Category
            {
                CategoryId = playGuid,
                Name = "Plays"
            });

            var johnEgbertGuid = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA317}");
            var nickSailorGuid = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA318}");
            var michaelJohnsonGuid = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA319}");

            // Mixed stock levels are intentional. The order flow's
            // reservation and compensation paths each need a deterministic
            // way to be triggered in demos:
            //   - 0 (Nick Sailor)         → sold-out path
            //   - 3 (John Egbert)         → small-stock path (good for over-asking)
            //   - 7 (Lighthouse Keeper's) → low stock
            //   - 100 (everything else)   → happy path
            modelBuilder.Entity<Event>().HasData(new Event
            {
                EventId = johnEgbertGuid,
                Name = "John Egbert Live",
                Price = 65,
                Artist = "John Egbert",
                Date = new DateTime(2026, 11, 1),
                Description = "Join John for his farwell tour across 15 continents. John really needs no introduction since he has already mesmerized the world with his banjo.",
                ImageUrl = "/img/banjo.jpg",
                CategoryId = concertGuid,
                TicketsAvailable = 3
            });


            modelBuilder.Entity<Ticket>().HasData(new Ticket()
            {
                EventId = johnEgbertGuid,
                Name = "Standard",
                Price = 65,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA31A}")
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket()
            {
                EventId = johnEgbertGuid,
                Name = "Premium",
                Price = 95,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA31B}")
            });

            modelBuilder.Entity<Event>().HasData(new Event
            {
                EventId = michaelJohnsonGuid,
                Name = "The State of Affairs: Michael Live!",
                Price = 85,
                Artist = "Michael Johnson",
                Date = new DateTime(2027, 2, 1),
                Description = "Michael Johnson doesn't need an introduction. His 25 concert across the globe last year were seen by thousands. Can we add you to the list?",
                ImageUrl = "/img/michael.jpg",
                CategoryId = concertGuid,
                TicketsAvailable = 100
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket()
            {
                EventId = michaelJohnsonGuid,
                Name = "Standard",
                Price = 85,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA31C}")
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket()
            {
                EventId = michaelJohnsonGuid,
                Name = "Premium",
                Price = 110,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA31D}")
            });


            modelBuilder.Entity<Event>().HasData(new Event
            {
                EventId = nickSailorGuid,
                Name = "To the Moon and Back",
                Price = 135,
                Artist = "Nick Sailor",
                Date = new DateTime(2027, 1, 1),
                Description = "The critics are over the moon and so will you after you've watched this sing and dance extravaganza written by Nick Sailor, the man from 'My dad and sister'.",
                ImageUrl = "/img/musical.jpg",
                CategoryId = musicalGuid,
                TicketsAvailable = 0
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket()
            {
                EventId = nickSailorGuid,
                Name = "Standard",
                Price = 135,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA31E}")
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket()
            {
                EventId = nickSailorGuid,
                Name = "Premium",
                Price = 190,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA31F}")
            });

            var aishaPatelGuid = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA320}");
            var mayaOkaforGuid = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA321}");
            var sunlightAvenueGuid = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA322}");
            var lighthouseGuid = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA323}");
            var lettersGuid = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA324}");

            modelBuilder.Entity<Event>().HasData(new Event
            {
                EventId = aishaPatelGuid,
                Name = "An Evening with Aisha Patel",
                Price = 70,
                Artist = "Aisha Patel",
                Date = new DateTime(2027, 3, 14),
                Description = "Aisha Patel returns to the city stage for one night only, blending classical violin with the rhythms of her South-Asian heritage. A warm, intimate evening that has sold out venues from London to Singapore.",
                ImageUrl = "/img/aisha.jpg",
                CategoryId = concertGuid,
                TicketsAvailable = 100
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket
            {
                EventId = aishaPatelGuid,
                Name = "Standard",
                Price = 70,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA325}")
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket
            {
                EventId = aishaPatelGuid,
                Name = "Premium",
                Price = 110,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA326}")
            });

            modelBuilder.Entity<Event>().HasData(new Event
            {
                EventId = mayaOkaforGuid,
                Name = "Midnight Sessions with Maya Okafor",
                Price = 80,
                Artist = "Maya Okafor",
                Date = new DateTime(2027, 5, 22),
                Description = "Three-time Grammy nominee Maya Okafor brings her signature blend of jazz, soul and contemporary R&B to the headline stage. Expect new material from her upcoming album alongside the songs you already love.",
                ImageUrl = "/img/maya.jpg",
                CategoryId = concertGuid,
                TicketsAvailable = 100
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket
            {
                EventId = mayaOkaforGuid,
                Name = "Standard",
                Price = 80,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA327}")
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket
            {
                EventId = mayaOkaforGuid,
                Name = "Premium",
                Price = 130,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA328}")
            });

            modelBuilder.Entity<Event>().HasData(new Event
            {
                EventId = sunlightAvenueGuid,
                Name = "Sunlight Avenue",
                Price = 120,
                Artist = "Priya Raman",
                Date = new DateTime(2027, 7, 4),
                Description = "A vibrant new musical from composer Priya Raman following four neighbours over one transformative summer. Critics have called it the freshest score Broadway has heard in years.",
                ImageUrl = "/img/sunlight.jpg",
                CategoryId = musicalGuid,
                TicketsAvailable = 100
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket
            {
                EventId = sunlightAvenueGuid,
                Name = "Standard",
                Price = 120,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA329}")
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket
            {
                EventId = sunlightAvenueGuid,
                Name = "Premium",
                Price = 180,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA32A}")
            });

            modelBuilder.Entity<Event>().HasData(new Event
            {
                EventId = lighthouseGuid,
                Name = "The Lighthouse Keeper's Daughter",
                Price = 55,
                Artist = "Helena Marsh",
                Date = new DateTime(2027, 9, 12),
                Description = "Helena Marsh's quietly devastating two-hander has won this year's Olivier Award for Best New Play. A lighthouse, a long-kept secret, and a daughter returning home after twenty years away.",
                ImageUrl = "/img/lighthouse.jpg",
                CategoryId = playGuid,
                TicketsAvailable = 7
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket
            {
                EventId = lighthouseGuid,
                Name = "Standard",
                Price = 55,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA32B}")
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket
            {
                EventId = lighthouseGuid,
                Name = "Premium",
                Price = 90,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA32C}")
            });

            modelBuilder.Entity<Event>().HasData(new Event
            {
                EventId = lettersGuid,
                Name = "Letters from the Border",
                Price = 60,
                Artist = "Kenji Tanaka",
                Date = new DateTime(2027, 10, 30),
                Description = "Kenji Tanaka's celebrated drama, translated into eleven languages, makes its long-awaited debut on the main stage. A correspondence between two strangers across a closed border, and what happens when the border finally opens.",
                ImageUrl = "/img/letters.jpg",
                CategoryId = playGuid,
                TicketsAvailable = 100
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket
            {
                EventId = lettersGuid,
                Name = "Standard",
                Price = 60,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA32D}")
            });

            modelBuilder.Entity<Ticket>().HasData(new Ticket
            {
                EventId = lettersGuid,
                Name = "Premium",
                Price = 95,
                TicketId = Guid.Parse("{CFB88E29-4744-48C0-94FA-B25B92DEA32E}")
            });
        }
    }

}

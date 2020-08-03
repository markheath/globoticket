using System;
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

            modelBuilder.Entity<Event>().HasData(new Event
            {
                EventId = johnEgbertGuid,
                Name = "John Egbert Live",
                Price = 65,
                Artist = "John Egbert",
                Date = DateTime.Now.AddMonths(6),
                Description = "Join John for his farwell tour across 15 continents. John really needs no introduction since he has already mesmerized the world with his banjo.",
                ImageUrl = "/img/banjo.jpg",
                CategoryId = concertGuid
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
                Date = DateTime.Now.AddMonths(9),
                Description = "Michael Johnson doesn't need an introduction. His 25 concert across the globe last year were seen by thousands. Can we add you to the list?",
                ImageUrl = "/img/michael.jpg",
                CategoryId = concertGuid
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
                Date = DateTime.Now.AddMonths(8),
                Description = "The critics are over the moon and so will you after you've watched this sing and dance extravaganza written by Nick Sailor, the man from 'My dad and sister'.",
                ImageUrl = "/img/musical.jpg",
                CategoryId = musicalGuid
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
        }
    }

}

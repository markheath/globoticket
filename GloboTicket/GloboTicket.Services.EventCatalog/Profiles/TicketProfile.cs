using AutoMapper;

namespace GloboTicket.Services.EventCatalog.Profiles
{
    public class TicketProfile : Profile
    {
        public TicketProfile()
        {
            CreateMap<Entities.Ticket, Models.TicketDto>().ReverseMap();
        }
    }

}

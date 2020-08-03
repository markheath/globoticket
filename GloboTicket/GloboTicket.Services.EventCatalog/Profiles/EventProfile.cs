using AutoMapper;
using System.Linq;

namespace GloboTicket.Services.EventCatalog.Profiles
{
    public class EventProfile: Profile
    {
        public EventProfile()
        {
            CreateMap<Entities.Event, Models.EventDto>()
                .ForMember(dest => dest.CategoryName, opts => opts.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Price, opts => opts.MapFrom(src => src.Tickets.Min(t => t.Price)));
            CreateMap<Entities.Event, Models.V2.EventDto>()
                .ForMember(dest => dest.CategoryName, opts => opts.MapFrom(src => src.Category.Name));
        }
    }

}

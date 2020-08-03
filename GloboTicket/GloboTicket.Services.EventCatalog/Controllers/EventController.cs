using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GloboTicket.Services.EventCatalog.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Services.EventCatalog.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public EventController(IEventRepository eventRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.EventDto>>> Get(
            [FromQuery] Guid categoryId)
        {
            var result = await _eventRepository.GetEvents(categoryId);
            return Ok(_mapper.Map<List<Models.EventDto>>(result));
        }

        [HttpGet("{eventId}")]
        public async Task<ActionResult<Models.EventDto>> GetById(Guid eventId)
        {
            var result = await _eventRepository.GetEventById(eventId);
            return Ok(_mapper.Map<Models.EventDto>(result));
        }

        [HttpGet]
        [MapToApiVersion("2.0")]
        public async Task<ActionResult<IEnumerable<Models.EventDtoV2>>> GetV2([FromQuery] Guid categoryId)
        {
            var result = await _eventRepository.GetEvents(categoryId);
            return Ok(_mapper.Map<List<Models.EventDtoV2>>(result));
        }

        [HttpGet("{eventId}")]
        [MapToApiVersion("2.0")]
        public async Task<ActionResult<Models.EventDtoV2>> GetByIdV2(Guid eventId)
        {
            var result = await _eventRepository.GetEventById(eventId);
            return Ok(_mapper.Map<Models.EventDtoV2>(result));
        }
    }
}
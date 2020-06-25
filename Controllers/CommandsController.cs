using System.Collections.Generic;
using AutoMapper;
using Commander.Data;
using Commander.Dtos;
using Commander.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace Commander.Controllers
{
    // api/commands
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private ICommanderRepo _repository;
        private IMapper _mapper;

        //private readonly MockCommanderRepo _repository = new MockCommanderRepo();

        public CommandsController(ICommanderRepo repository, IMapper mapper) 
        {
            _repository = repository;
            _mapper = mapper;
        }

        // GET api/commands
        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetAllCommands() 
        {
            var commandItems = _repository.GetAllCommands();
            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));
        }
        
        // GET api/commands/{id}
        // Note: This must be explicitedly named to be used by Create
        [HttpGet("{id}", Name="GetCommandById")]
        public ActionResult<CommandReadDto> GetCommandById(int id) 
        {
            var commandItem = _repository.GetCommandById(id);
            if (commandItem != null) 
            {                
                return Ok(_mapper.Map<CommandReadDto>(commandItem));
            }
            return NotFound();
        }

        // POST api/commands
        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommand(CommandCreateDto commandCreateDto) 
        {
            // TODO: Validation
            var commandModel = _mapper.Map<Command>(commandCreateDto);
            _repository.CreateCommand(commandModel);
            _repository.SaveChanges();

            var commandReadDto = _mapper.Map<CommandReadDto>(commandModel);
            
            return CreatedAtRoute(nameof(GetCommandById), new {Id = commandReadDto.Id}, commandReadDto);
        }


        // TODO: PUT is returning a 405. Unclear why

        //PUT api/commands/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateCommand(int id, [FromBody]CommandUpdateDto commandUpdateDto)
        {
            var commandModelFromRepo = _repository.GetCommandById(id);
            if (commandModelFromRepo == null)
            {
                return NotFound();
            }
            _mapper.Map(commandUpdateDto, commandModelFromRepo);

            _repository.UpdateCommand(commandModelFromRepo);

            _repository.SaveChanges();

            return NoContent();
        }
        
        // PATCH api/commands/{id}
        [HttpPatch("{id}")]
        public IActionResult PartialCommandUpdate(int id, [FromBody]JsonPatchDocument<CommandUpdateDto> patchDoc)
        {
            var commandModelFromRepo = _repository.GetCommandById(id);
            if (commandModelFromRepo == null)
            {
                return NotFound();
            }

            var commandToPatch = _mapper.Map<CommandUpdateDto>(commandModelFromRepo);
            patchDoc.ApplyTo(commandToPatch, ModelState);
            if (!TryValidateModel(commandToPatch))
            {
                return ValidationProblem(ModelState);
            }
            
            _mapper.Map(commandToPatch, commandModelFromRepo);

            _repository.UpdateCommand(commandModelFromRepo);
            _repository.SaveChanges();

            return NoContent();
        }

        
        // DELETE api/commands/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteCommand(int id) 
        {
            var commandModelFromRepo = _repository.GetCommandById(id);
            if (commandModelFromRepo == null) 
            {
                return NotFound();
            }

            _repository.DeleteCommand(commandModelFromRepo);
            _repository.SaveChanges();

            return Ok();
        }
    }
}
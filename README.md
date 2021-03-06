# tutorial_commander
An example of setting up a .Net Core 3.1 Web Api combining tutorials by Les Jackson and DotNet Core Central

This should be a very basic rundown of 
- A Web Api using EntityFramework/SQL Server Express
- AutoMapper to map the Models to DTOs
- Authentication and Authorization using JWTs and AspNetCore Authenticaiton to handle it
- All kinds of Web Api things that I was clueless about when I spun up an API from the void years ago


From Les Jackson's youtube video on Web API in .Net Core 3.1:
https://www.youtube.com/watch?v=fmvcAzHpsk8

From DotNet Core Central's video on JWT authentication:
https://www.youtube.com/watch?v=vWkPdurauaA

# Quick notes I took when creating this app



****************************************************************************
** Plumbing: Setting up base architecture
****************************************************************************

    [ Client ] <------>  [ Controller ] <-----> [ Repository ] 
                 (JSON)          ^                   ^
                                 | Uses/Returns      |
                                 v                   |
                              [ DTO ]                |
                                 |                   |
                                 | AutoMapper        |
                                 v                   v
                             [ Model ] <-----> [ DbContext ] 
                                                     |
                                               [ Database ]
                                                     

	--------------------------------------------------
	- Setup
	--------------------------------------------------

		* Download .Net Core 3.1
		* VS Code
		* SQL Server Express (If using this as the db)
		* SQL Server Management Studio

		> dotnet new webapi -n <ProjectName>
		> code -r <projectname>
		
		Delete scaffolding
			* files in controllers
			* model in base folder
			

	--------------------------------------------------
	- Project layout
	--------------------------------------------------

		* Program.cs - Creates the host and startup class
		* Startup.cs - Central hub of the app
			- Setup
			- ConfigureServices() - where to add configuration
			- Configure() - where to setup request pipeline (chain of middleware that handles the request. Order matters)
		* .csproj - project dependencies
		* appsettings.json / appsettings.Development.json - config 
		* launchSettings.Json - which environment, URL
		
	--------------------------------------------------
	- Running the WebAPI
	--------------------------------------------------
		> dotnet build
		> dotnet run

	--------------------------------------------------
	- Models
	--------------------------------------------------

		* Representation of the main data
		+ Create a /models folder
		+ Create a <modelName.cs> file
		
			namespace <ProjectName>.Models
			{
				public class <FileName> 
				{
					<properties>
				}
			}

	--------------------------------------------------
	- Repository
	--------------------------------------------------

		* Provides a contract (Interface) that defines what the implemnter should (and must) implement
		* Interface itself doesn't care how its implemented, decouples concerns from the interface
		* List of method signatures
			
		+ Create a /data folder
		+ Create an Interface: <INameRepo.cs>	
			* Add methods

	--------------------------------------------------
	- Controller
	--------------------------------------------------

		* Generally, one controller per resource
		* In /controllers
		* Resource name generally plural
		+ Create <resourceController.cs>
			
		* Controllers inherit from ControllerBase
			- ex:
			
				// api/commands
				[Route("api/[controller]")]
				[ApiController]
				public class CommandsController : ControllerBase
				{
				}
		
		* Generally, controllers should implement
			/api/commands
				* Read all resources
				* GET
				* READ
				* 200 / 400 bad request / 404 not found
			/api/commands/{id}
				* Read a single resources
				* GET
				* READ
				* 200 / 400 bad request / 404 not found
			/api/commands
				* Create a new resource
				* POST
				* CREATE
				* 201 / 400 bad request / 405 not allowed
			/api/commands/{id}
				* Update an entire resources
				* PUT (PATCH for partial)
				* UPDATE
				* 204 no content ..
			/api/commands/{id}
				* Deletes a single resources
				* DELETE
				* 200 / 204 no content / ..
			/api/commands/
				* DELETE
				* Deletes all resources 
			
		* Try to make names self-documenting

	--------------------------------------------------
	- EXAMPLE: Implementing a Mock Repository 
	- (Optional)
	--------------------------------------------------

		* When creating a mock repository / fake data that implements the Interface
		* In /data
		+ Create a <MockRepoInterface.cs> class
			
			using System.Collections.Generic;
			using Commander.Models;

			namespace Commander.Data
			{
				public class MockCommanderRepo : ICommanderRepo
				{
					public IEnumerable<Command> GetAllCommands()
					{
						var commands = new List<Command>
						{
							new Command{Id=0, HowTo="First command", Line="Item1", Platform="Platform1"},
							new Command{Id=1, HowTo="Second command", Line="Item2", Platform="Platform2"},
							new Command{Id=2, HowTo="Third command", Line="Item3", Platform="Platform3"},
						};

						return commands;
					}

					public Command GetCommandById(int id) 
					{
						return new Command{Id=0, HowTo="First command", Line="Item1", Platform="Platform1"};
					}
				}
			}
			
		* ex: CommandsController.cs
		
				using System.Collections.Generic;
				using Commander.Data;
				using Commander.Models;
				using Microsoft.AspNetCore.Mvc;

				namespace Commander.Controllers
				{
					// api/commands
					[Route("api/[controller]")]
					[ApiController]
					public class CommandsController : ControllerBase
					{
						private ICommanderRepo _repository;
						//private readonly MockCommanderRepo _repository = new MockCommanderRepo();

						public CommandsController(ICommanderRepo repository) 
						{
							_repository = repository;
						}
						// GET api/commands
						[HttpGet]
						public ActionResult<IEnumerable<Command>> GetAllCommands() 
						{
							var commandItems = _repository.GetAppCommands();
							return Ok(commandItems);
						}
						
						// GET api/commands/{id}
						[HttpGet("{id")]
						public ActionResult<Command> GetCommandById(int id) 
						{
							var commandItem = _repository.GetCommandById(id);
							if (commandItem != null) 
							{                
								return Ok(commandItem);
							}
							return NotFound();
						}
					}
				}

	--------------------------------------------------
	- Startup ConfigureServices
	- ** Dependency Injection
	--------------------------------------------------

		* Startup.ConfigureServices
			- Configures repositories in one place
			** Decouples this from where its injected - if a Repository is swapped out, the rest of the app does not need to change 
			  (the method signatures from the interface are the same)
			- If repositories are 'swapped out'
			
		* ex: Registering a Scoped service (Scoped: Service is created per client request)
			
			public void ConfigureServices(IServiceCollection services)
			{
				services.AddControllers();

				// 3 ways to register a service
				// - AddSingleton: Same object for every request
				// - AddScoped: Same object for one request (one client req)
				// - Transient: New instance created every time
				services.AddScoped<ICommanderRepo, MockCommanderRepo>();
			}
		
	--------------------------------------------------
	- Database - SQL Server Express
	--------------------------------------------------

		* Create a new SQL account for the WebAPI
		* In Sql Server Management Studio
			- Right-click the Server -> Properties
				* Server authentication -> Security
				* (x) SQL Server and Windows Authentication mode
				* STOP and START the server
			- Create a new database
			- Security -> Logins
				* Create a new user
				* Under [Server Roles] give it sysadmin (or some other access role)
			- Test the new user by disconnecting/reconnecting
			- Create a new Database
		
		* In the WebApp
		* In appsettings.json, add 
			"ConnectionStrings": 
			{
				"MyConnectionString": "Server=<server\\SQLEXPRESS;Initial Catalog=<DB Name>;User ID=CommanderAPI;Password=nuclearcommander"
			}
			
		** Should use a secrets file for passwords
		
	--------------------------------------------------
	- EntityFramework
	--------------------------------------------------

		* nuget
		> dotnet add package Microsoft.EntityFrameworkCore
		> dotnet add package Microsoft.EntityFrameworkCore.Design
		> dotnet add package Microsoft.EntityFrameworkCore.SqlServer
		> dotnet tool install --global dotnet-ef
		
	--------------------------------------------------
	- DbContext
	--------------------------------------------------

		* Mediator for connecting the database to the repository/model
		* In /data
		+ Create a <resourceNameContext.cs> class
		
		* Inherits from DbContext, call the constructor
		* Ex:
				using Commander.Models;
				using Microsoft.EntityFrameworkCore;

				namespace Commander.Data
				{
					public class CommanderContext : DbContext
					{
						public CommanderContext(DbContextOptions<CommanderContext> opt) : base(opt) {}
						public DbSet<Command> Commands { get; set; }
					}
				}
		
		* In startup.cs/ConfigureServices()
		* Add before services.AddControllers();
		
			services.AddDbContext<MyContext>(opt => opt.UseSqlServer
					(Configuration.GetConnectionString("myConnection")));

	--------------------------------------------------
	- Ef Migrations - Code First
	--------------------------------------------------

		* First migration example
			> dotnet ef migrations add InitialMigration	
				- This should create a migration file in a /Migrations folder
				- Up/Down what commands this runs on the SQL Server when applied/removed
			
			> dotnet ef database update

	--------------------------------------------------
	- Database repository 
	--------------------------------------------------

		* In /data
		* Create a <resourceNameRepo.cs> class that implements the Interface
		* Use Dependency injection to get the dbcontext in the repo's constructor
		* Ex:
		
			using System.Collections.Generic;
			using System.Linq;
			using Commander.Models;

			namespace Commander.Data
			{
				public class SqlCommanderRepo : ICommanderRepo
				{
					private CommanderContext _context;

					public SqlCommanderRepo(CommanderContext context) {
						_context = context;
					}

					IEnumerable<Command> ICommanderRepo.GetAllCommands()
					{
						return _context.Commands.ToList();
					}
					public Command GetCommandById(int id)
					{
						return _context.Commands.FirstOrDefault(p => p.Id == id);
					}
				}
			}
		
		

	--------------------------------------------------
	- Models - some EF data annotations
	--------------------------------------------------

		* Annotations can further define or restrict data table column behavior 

			ex:		[Key] <- Explicitedly sets the Id 
					[Required] <- Non-nullable
					[MaxLength(250)] <- Max string length
					...
					public string Foo { get; set; }

	--------------------------------------------------
	- Current setup
	--------------------------------------------------

		* Startup.cs
			- Set up a [DbContext] / EntityFramework to connect to the database
			- Set up a [Repository] as a bridge to the DbConext and return [Model] data
			- Set up [Controllers] that use the Repository to fetch data
		
		* Current issues
			- Exposing domain model details directly through the API (irrelevant, insecure data, wrong format, etc)
			- Coupling internal / external data (hard to change model structure)
			
		* Solutions
			- Use a [Data Transfer Object] (DTO) to decouple internal models from external models
				
	--------------------------------------------------
	- Create a DTO with AutoMapper
	--------------------------------------------------

		* To separate internal models from external data representations
		* AutoMapper isn't mandatory for small objects, but doing it manually is very error-prone
		
		* Install packages
			> dotnet add package Auto<apper.Extensions.Microsoft.DependencyInjection
			
		* Add to Startup.ConfigureServices()
			- After services.AddControllers();
			- services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
			
		+ Create a /Dtos folder
		+ Create a <dtoNameReadDto.cs> file
		+ Create a /Profiles folder
		+ Create a <dtoNamesProfile.cs> file
			* Inherit from Profile
			
		Ex:
			- Models/Command.cs
			
				public class Command 
				{
					public int Id { get; set; }
					public string HowTo { get; set; }
					public string Line { get; set; }
					public string Platform { get; set; }
				}
				
			- Dtos/CommandReadDto.cs
			
				public class CommandReadDto
				{
					public int Id { get; set; }
					public string HowTo { get; set; }
					public string Line { get; set; }
				}
			
			- Profiles/CommandsProfile.cs
			
				public class CommandsProfile : Profile
				{
					public CommandsProfile()
					{
						CreateMap<Command, CommandReadDto>();
					}
				}
				
		* AutoMapper should automatically map some of the simpler mappings
		
	--------------------------------------------------
	- Controller - DI to add AutoMapper
	--------------------------------------------------

		* Add IMapper to the Controller constructor and add a private field
		* Replace model signatures with the DTO class
		
		Ex: CommandsController.cs
		
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
			
			[HttpGet("{id")]
			public ActionResult<CommandReadDto> GetCommandById(int id) 
			{
				var commandItem = _repository.GetCommandById(id);
				if (commandItem != null) 
				{                
					return Ok(_mapper.Map<CommandReadDto>(commandItem));
				}
				return NotFound();
			}	

****************************************************************************
** More CRUD operations
****************************************************************************

	--------------------------------------------------
	- IRepo - Add Create and SaveChanges
	--------------------------------------------------
	
		* Add void CreateModelName();
		* Add bool SaveChanges();
			- A common method to determine if EF changes were applied to the database
		
		Ex:		
			public void CreateCommand(Command cmd)
			{
				if (cmd == null) {
					throw new ArgumentNullException(nameof(cmd));
				}
				_context.Commands.Add(cmd);
				_context.SaveChanges();
			}
			
			public bool SaveChanges()
			{
				return (_context.SaveChanges() >= 0);
			}


	--------------------------------------------------
	- Add a CreateDto
	--------------------------------------------------
	
		Ex:		
			public class CommandCreateDto
			{
				public string HowTo { get; set; }

				public string Line { get; set; }
				
				public string Platform { get; set; }
			}
		
			* Note this Dto does not have the Id field, in this instance it isn't passed by the client and handled by the server 
			
	--------------------------------------------------
	- Add a mapping to CreateDto to the Model to the Profile
	--------------------------------------------------
	
		Ex:
			public class CommandsProfile : Profile
			{
				public CommandsProfile()
				{
					// source -> target
					CreateMap<Command, CommandReadDto>();
					CreateMap<CommandCreateDto, Command>();
				}
			}

	--------------------------------------------------
	- Add a Create method to the controller
	--------------------------------------------------
	
		Ex:
		
			[HttpGet("id", Name="GetCommandById")]
			public ActionResult<CommandReadDto> GetCommandById(int id) {...}
		
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
		
			** Part of the REST specification is to return the URI to that resource
			** The get route needs to be explicitedly named to pass it to the CreatedByRoute
			
	--------------------------------------------------
	- Handling Updates (PUT, PATCH, DELETE)
	--------------------------------------------------
	
		* Add annotations where needed to the CreateDto.cs 
			- This way, it will return a 400 with invalid input/bad request instead of an ungraceful error
			
		* Add methods to the repo
			- The repo is agnostic; name it from a logical perspective, not from a specific technology implementing it perspective
			
		* Add Mapping to the Profile
			ex: 			
				CreateMap<CommandUpdateDto, Command>(); // Update
				CreateMap<Command, CommandUpdateDto>(); // Patch
		
		* PATCH - 
			> dotnet add package Microsoft.AspNetCore.JsonPatch
			> dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson 
				- JSON Serialization
				
			- Startup: Add to services.AddControllers() 
				services.AddControllers().AddNewtonsoftJson(s => {
					s.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
				});
				
			
		* Add the Controller methods
		
		Ex:
			
	        // PUT api/commands/{id}
			[HttpPut("{id}")]
			public ActionResult UpdateCommand(int id, [FromBody]CommandUpdateDto commandUpdateDto) 
			{
				var commandModelFromRepo = _repository.GetCommandById(id);
				if (commandModelFromRepo == null)
				{
					return NotFound();
				}

				// This mapping also updates the model
				_mapper.Map(commandUpdateDto, commandModelFromRepo);

				// Doesn't do anything in this case, but good practice to have it 
				_repository.UpdateCommand(commandModelFromRepo);
				_repository.SaveChanges();

				return NoContent();
			}
				
			// PATCH api/commands/{id}
			[HttpPatch("{id}")]
			public ActionResult PartialCommandUpdate(int id, JsonPatchDocument<CommandUpdateDto> patchDoc)
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
			
			
****************************************************************************
** Authentication with JWT
****************************************************************************

	--------------------------------------------------
	- Packages
	--------------------------------------------------
    * nuget
		> dotnet add package Microsoft.AspNetCore.Authentication
            - Authentication
		> dotnet add package System.IdentityModel.Tokens.Jwt
            - JWT creation 
		> dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
            - Middlewear that lets an app receive an OpenID Connect bearer token

	--------------------------------------------------
	- Startup
	--------------------------------------------------

        Startup.Configure()
            - Before app.UseAuthorization();
            - Add app.UseAuthentication();

	--------------------------------------------------
	- Create an Auth controller / UserCredentials class
	--------------------------------------------------

        * In controllers
        + Add a LoginController.cs
            - Add an Authenticate method with a [FromBody]UserCred input
        + Add a UserCred.cs class
            - Add Username, Password as props


        ex: 
            [Route("api/[controller]")]
            [ApiController]
            public class LoginController : ControllerBase
            {
                [HttpPost("authenticate")]
                public IActionResult Authenticate([FromBody]UserCred userCred)
                {
                    return Ok();
                }
            }

	--------------------------------------------------
	- Create an AuthenticationManager to handle authentication
	--------------------------------------------------

		+ Create a IJwtAuthenticationManager.cs interface with an Authenticate method
			- This should return a string representing a JWT

		+ Create a JwtAuthenticationManager.cs to implement this
			- This should authenticate the user, and if successful, return a JWT

		ex:
			public class JwtAuthenticationManager : IJwtAuthenticationManager
			{
				// TODO: Move this to the database
				private readonly IDictionary<string, string> users = new Dictionary<string, string>
				{
					{ "admin", "pass" },
					{ "user", "pass" }
				};
				private readonly string key;

				// Private key to encrypt the Jwt token
				public JwtAuthenticationManager(string key) 
				{
					this.key = key;
				}

				public string Authenticate(string username, string password)
				{
					if (!users.Any(u => u.Key == username && u.Value == password))
					{
						return null;
					}

					var tokenHandler = new JwtSecurityTokenHandler();
					var tokenKey = Encoding.ASCII.GetBytes(key);

					// Define the token
					var tokenDescriptor = new SecurityTokenDescriptor
					{
						Subject = new ClaimsIdentity(new Claim[] {
							new Claim(ClaimTypes.Name, username)
						}),
						Expires = DateTime.UtcNow.AddHours(24),
						// How the token is signed
						SigningCredentials = new SigningCredentials(
							new SymmetricSecurityKey(tokenKey), 
							SecurityAlgorithms.HmacSha256Signature
						)
					};
					
					var token = tokenHandler.CreateToken(tokenDescriptor);
					
					return tokenHandler.WriteToken(token);
				}
			}

	--------------------------------------------------
	- Startup - Add Authentication
	--------------------------------------------------
		
		* In Startup.ConfigureServices
			- After services.AddControllers()
			- Add services.AddAuthentication() --

				// TODO: Move this and use a more secure key
				var key = "Test key";

				// Middleware that handles authentication and adds the Jwt bearer
				services.AddAuthentication(x => 
				{
					x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				}).AddJwtBearer(x => 
				{
					x.RequireHttpsMetadata = false;
					x.SaveToken = true;
					x.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
						ValidateIssuer = false,
						ValidateAudience = false
					};
				});
				
			- Add services.AddSingleton<IJwtAuthenticationManager>(new JwtAuthenticationManager(key));
				

	--------------------------------------------------
	- Controller - Add Authentication
	--------------------------------------------------
		
		* Use DI to add the JwtAuthenticationManager
		* Ex:
			[Route("api/[controller]")]
			[ApiController]
			public class LoginController : ControllerBase
			{
				private readonly IJwtAuthenticationManager jwtAuthenticationManager;

				public LoginController(IJwtAuthenticationManager jwtAuthenticationManager) {
					this.jwtAuthenticationManager = jwtAuthenticationManager;
				}

				[AllowAnonymous]
				[HttpPost("authenticate")]
				public IActionResult Authenticate([FromBody]UserCred userCred)
				{
					string token = jwtAuthenticationManager.Authenticate
						(userCred.UserName, userCred.Password);

					if (token != null)
					{
						return Ok(token);
					}
					
					return Unauthorized();            
				}
			}

	--------------------------------------------------
	- Controller - Add [Authorization] attributes
	--------------------------------------------------
		* In Controllers that require a login, add the [Authorization] annotation 
			- Can be at the Method or Class level			


	--------------------------------------------------
	- Testing in Postman
	--------------------------------------------------		
		* Send a post request to /api/login/authentication
			- In the body (Json), pass { "user": "x", "password": "x" }
			- If successful, copy the authentication token

		* Send a request to an authenticated route
			- In the Headers, add "Authorization": "Bearer <token>"
			- If successful, this should call the method


	--------------------------------------------------
	- Applying roles to Authorization
	--------------------------------------------------		
		* A controller's annotation can optionally contain one or more roles
			ex: 
        		[Authorize(Roles = "Administrator, User")]

		* When creating a JWT for a user, roles can be added through the token's claims
			ex: 
				new Claim(ClaimTypes.Role, "Administrator")

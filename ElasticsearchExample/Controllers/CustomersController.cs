using AutoMapper;
using ElasticsearchExample.Data.Repos;
using ElasticsearchExample.Dtos;
using ElasticsearchExample.Models.QueryParameters;
using ElasticsearchExample.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ILogger<CustomersController> _logger;
        private readonly ICustomersRepository _customersRepository;
        private readonly ICustomersSearchService _customersSearchService;
        private readonly IMapper _mapper;

        public CustomersController(
            ILogger<CustomersController> logger,
            ICustomersRepository customersRepository,
            ICustomersSearchService customersSearchService,
            IMapper mapper)
        {
            _logger = logger;
            _customersRepository = customersRepository;
            _customersSearchService = customersSearchService;
            _mapper = mapper;
        }

        [HttpGet(Name = nameof(GetCustomers))]
        public ActionResult<IEnumerable<CustomerReadDto>> GetCustomers(
            [FromQuery] QueryParameters queryParameters,
            CancellationToken cancellationToken)
        {
            var customersPage = _customersRepository
                .GetAll()
                .AsQueryable()
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Skip((queryParameters.PageNumber - 1) * queryParameters.QuerySize)
                .Take(queryParameters.QuerySize);

            return Ok(_mapper.Map<IEnumerable<CustomerReadDto>>(customersPage));
        }

        [HttpGet("search/byName", Name = nameof(GetCustomersByName))]
        public async Task<ActionResult<IEnumerable<CustomerReadDto>>> GetCustomersByName(
            [FromQuery] SearchQueryParameters queryParameters,
            CancellationToken cancellationToken)
        {
            var foundCustomers = await _customersSearchService.SearchByName(
                queryParameters.Value,
                (queryParameters.PageNumber - 1) * queryParameters.QuerySize,
                queryParameters.QuerySize,
                cancellationToken);

            if (foundCustomers is null || !foundCustomers.Any())
                return NotFound();

            var customerIds = foundCustomers!.Select(x => x.Id).ToList();

            var queryResults = _customersRepository
                .GetAll().AsQueryable().AsNoTracking()
                .Where(x => customerIds.Contains(x.Id))
                .OrderBy(c => c.Name)
                .Skip((queryParameters.PageNumber - 1) * queryParameters.QuerySize)
                .Take(queryParameters.QuerySize);

            return Ok(_mapper.Map<IEnumerable<CustomerReadDto>>(queryResults));
        }

        [HttpGet("search/bySurname", Name = nameof(GetCustomersBySurname))]
        public async Task<ActionResult<IEnumerable<CustomerReadDto>>> GetCustomersBySurname(
            [FromQuery] SearchQueryParameters queryParameters,
            CancellationToken cancellationToken)
        {
            var foundCustomers = await _customersSearchService.SearchBySurname(
                queryParameters.Value,
                (queryParameters.PageNumber - 1) * queryParameters.QuerySize,
                queryParameters.QuerySize,
                cancellationToken);

            if (foundCustomers is null || !foundCustomers.Any())
                return NotFound();

            var customerIds = foundCustomers!.Select(x => x.Id).ToList();

            var queryResults = _customersRepository
                .GetAll().AsQueryable().AsNoTracking()
                .Where(x => customerIds.Contains(x.Id))
                .OrderBy(c => c.Name)
                .Skip((queryParameters.PageNumber - 1) * queryParameters.QuerySize)
                .Take(queryParameters.QuerySize);

            return Ok(_mapper.Map<IEnumerable<CustomerReadDto>>(queryResults));
        }

        [HttpGet("search/byPhoneNumber", Name = nameof(GetCustomersByPhoneNumber))]
        public async Task<ActionResult<IEnumerable<CustomerReadDto>>> GetCustomersByPhoneNumber(
            [FromQuery] SearchQueryParameters queryParameters,
            CancellationToken cancellationToken)
        {
            var foundCustomers = await _customersSearchService
                .SearchByPhoneNumber(queryParameters.Value,
                (queryParameters.PageNumber - 1) * queryParameters.QuerySize,
                queryParameters.QuerySize,
                cancellationToken);

            if (foundCustomers is null || !foundCustomers.Any())
                return NotFound();

            var customerIds = foundCustomers!.Select(x => x.Id).ToList();

            var queryResults = _customersRepository
                .GetAll().AsQueryable().AsNoTracking()
                .Where(x => customerIds.Contains(x.Id))
                .OrderBy(c => c.PhoneNumber)
                .Skip((queryParameters.PageNumber - 1) * queryParameters.QuerySize)
                .Take(queryParameters.QuerySize);

            return Ok(_mapper.Map<IEnumerable<CustomerReadDto>>(queryResults));
        }

        [HttpGet("search/byHobby", Name = nameof(GetCustomersByHobby))]
        public async Task<ActionResult<IEnumerable<CustomerReadDto>>> GetCustomersByHobby(
            [FromQuery] SearchQueryParameters queryParameters,
            CancellationToken cancellationToken)
        {
            var foundCustomers = await _customersSearchService
                .SearchByHobby(queryParameters.Value,
                (queryParameters.PageNumber - 1) * queryParameters.QuerySize,
                queryParameters.QuerySize,
                cancellationToken);

            if (foundCustomers is null || !foundCustomers.Any())
                return NotFound();

            var customerIds = foundCustomers!.Select(x => x.Id).ToList();

            var queryResults = _customersRepository
                .GetAll().AsQueryable().AsNoTracking()
                .Where(x => customerIds.Contains(x.Id))
                .OrderBy(c => c.Hobbies)
                .Skip((queryParameters.PageNumber - 1) * queryParameters.QuerySize)
                .Take(queryParameters.QuerySize);

            return Ok(_mapper.Map<IEnumerable<CustomerReadDto>>(queryResults));
        }

        [HttpGet("count/withHobby/{hobby}", Name = nameof(GetCountOfCustomersWithHobby))]
        public async Task<object> GetCountOfCustomersWithHobby(
            [FromRoute] string hobby,
            CancellationToken cancellationToken)
        {
            int count = await _customersSearchService.CountCustomersByHobby(hobby, cancellationToken);
            return Ok(new { hobby, count });
        }
    }
}

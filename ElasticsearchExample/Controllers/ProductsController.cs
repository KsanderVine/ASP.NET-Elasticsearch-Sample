using AutoMapper;
using ElasticsearchExample.Data.Repos;
using ElasticsearchExample.Dtos;
using ElasticsearchExample.Models.Buckets;
using ElasticsearchExample.Models.QueryParameters;
using ElasticsearchExample.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IProductsRepository _productsRepository;
        private readonly IProductsSearchService _productsSearchService;
        private readonly IMapper _mapper;

        public ProductsController(
            ILogger<ProductsController> logger,
            IProductsRepository productsRepository,
            IProductsSearchService productsSearchService,
            IMapper mapper)
        {
            _logger = logger;
            _productsRepository = productsRepository;
            _productsSearchService = productsSearchService;
            _mapper = mapper;
        }

        [HttpGet(Name = nameof(GetProducts))]
        public IEnumerable<ProductReadDto> GetProducts(
            [FromQuery] QueryParameters queryParameters)
        {
            var productsPage = _productsRepository
                .GetAll()
                .AsQueryable()
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Skip((queryParameters.PageNumber - 1) * queryParameters.QuerySize)
                .Take(queryParameters.QuerySize);

            return _mapper.Map<IEnumerable<ProductReadDto>>(productsPage);
        }

        [HttpGet("search/byName", Name = nameof(GetProductsByName))]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetProductsByName(
            [FromQuery] SearchQueryParameters queryParameters,
            CancellationToken cancellationToken)
        {
            var foundProducts = await _productsSearchService.SearchByName(
                queryParameters.Value,
                (queryParameters.PageNumber - 1) * queryParameters.QuerySize,
                queryParameters.QuerySize,
                cancellationToken);

            if (foundProducts is null || !foundProducts.Any())
                return NotFound();

            var productsIds = foundProducts!.Select(x => x.Id).ToList();

            var queryResults = _productsRepository
                .GetAll().AsQueryable().AsNoTracking()
                .Where(x => productsIds.Contains(x.Id))
                .OrderBy(c => c.Name)
                .Skip((queryParameters.PageNumber - 1) * queryParameters.QuerySize)
                .Take(queryParameters.QuerySize);

            return Ok(_mapper.Map<IEnumerable<ProductReadDto>>(queryResults));
        }

        [HttpGet("search/byCategory", Name = nameof(GetProductsByCategory))]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetProductsByCategory(
            [FromQuery] SearchQueryParameters queryParameters,
            CancellationToken cancellationToken)
        {
            var foundProducts = await _productsSearchService.SearchByCategory(
                queryParameters.Value,
                (queryParameters.PageNumber - 1) * queryParameters.QuerySize,
                queryParameters.QuerySize,
                cancellationToken);

            if (foundProducts is null || !foundProducts.Any())
                return NotFound();

            var productsIds = foundProducts!.Select(x => x.Id).ToList();

            var queryResults = _productsRepository
                .GetAll().AsQueryable().AsNoTracking()
                .Where(x => productsIds.Contains(x.Id))
                .OrderBy(c => c.Name)
                .Skip((queryParameters.PageNumber - 1) * queryParameters.QuerySize)
                .Take(queryParameters.QuerySize);

            return Ok(_mapper.Map<IEnumerable<ProductReadDto>>(queryResults));
        }

        [HttpGet("aggregate/byCategories", Name = nameof(GetAggregationByCategories))]
        public async Task<ActionResult<IEnumerable<ProductsByCategoryBucket>>> GetAggregationByCategories(
            CancellationToken cancellationToken)
        {
            var aggregation = await _productsSearchService
                .AggregateByCategories(cancellationToken);

            if (aggregation is null || !aggregation.Any())
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(aggregation);
        }
    }
}

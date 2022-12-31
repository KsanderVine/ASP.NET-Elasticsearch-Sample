using AutoMapper;
using Elastic.Clients.Elasticsearch.Aggregations;
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
    public class OrderItemsController : ControllerBase
    {
        private readonly ILogger<OrderItemsController> _logger;
        private readonly IOrderItemsRepository _orderItemsRepository;
        private readonly IOrderItemsSearchService _orderItemsSearchService;
        private readonly IMapper _mapper;

        public OrderItemsController(
            ILogger<OrderItemsController> logger,
            IOrderItemsRepository orderItemsRepository,
             IOrderItemsSearchService orderItemsSearchService,
            IMapper mapper)
        {
            _logger = logger;
            _orderItemsRepository = orderItemsRepository;
            _orderItemsSearchService = orderItemsSearchService;
            _mapper = mapper;
        }

        [HttpGet(Name = nameof(GetOrderItems))]
        public IEnumerable<OrderItemReadDto> GetOrderItems([FromQuery] QueryParameters queryParameters)
        {
            var orderItemsPage = _orderItemsRepository
                .GetAll()
                .AsQueryable()
                .AsNoTracking()
                .OrderBy(c => c.CreatedAt)
                .Skip((queryParameters.PageNumber - 1) * queryParameters.QuerySize)
                .Take(queryParameters.QuerySize);

            return _mapper.Map<IEnumerable<OrderItemReadDto>>(orderItemsPage);
        }

        [HttpGet("aggregate/by{interval}", Name = nameof(GetAggregationByInterval))]
        public async Task<ActionResult<IEnumerable<OrderItemsByIntervalBucket>>> GetAggregationByInterval(
            [FromRoute] CalendarInterval interval,
            CancellationToken cancellationToken)
        {
            var aggregation = await _orderItemsSearchService
                .AggregateByInterval(interval, cancellationToken);

            if (aggregation is null || !aggregation.Any())
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(aggregation);
        }

        [HttpGet("aggregate/{productId}/by{interval}", Name = nameof(GetAggregationForProductIdByInterval))]
        public async Task<ActionResult<IEnumerable<OrderItemsByIntervalBucket>>> GetAggregationForProductIdByInterval(
            [FromRoute] Guid productId,
            [FromRoute] CalendarInterval interval,
            CancellationToken cancellationToken)
        {
            var aggregation = await _orderItemsSearchService
                .AggregateForProductIdByInterval(productId, interval, cancellationToken);

            if (aggregation is null || !aggregation.Any())
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(aggregation);
        }

        [HttpGet("aggregate/{productId}/{customerId}/by{interval}", Name = nameof(GetAggregationForProductIdAndCustomerIdByInterval))]
        public async Task<ActionResult<IEnumerable<OrderItemsByIntervalBucket>>> GetAggregationForProductIdAndCustomerIdByInterval(
            [FromRoute] Guid productId,
            [FromRoute] Guid customerId,
            [FromRoute] CalendarInterval interval,
            CancellationToken cancellationToken)
        {
            var aggregation = await _orderItemsSearchService
                .AggregateForProductIdAndCustomerIdByInterval(productId, customerId, interval, cancellationToken);

            if (aggregation is null || !aggregation.Any())
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(aggregation);
        }
    }
}

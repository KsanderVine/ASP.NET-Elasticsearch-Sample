using ElasticsearchExample.Models;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchExample.Data.Repos
{
    public class OrderItemsRepository : IOrderItemsRepository
    {
        private readonly ILogger<OrderItemsRepository> _logger;
        private readonly AppDbContext _context;

        public OrderItemsRepository(
            ILogger<OrderItemsRepository> logger,
            AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void Create(OrderItem entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _context.Orders.Add(entity);
        }

        public async Task CreateAsync(OrderItem entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Orders.AddAsync(entity);
        }

        public void Delete(OrderItem entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _context.Orders.Remove(entity);
        }

        public IEnumerable<OrderItem> GetAll()
        {
            return _context.Orders;
        }

        public OrderItem? GetById(Guid id)
        {
            return _context.Orders.FirstOrDefault(o => o.Id == id);
        }

        public bool Save()
        {
            try
            {
                var stateEntires = _context.SaveChanges();
                return stateEntires > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while save changes in database");
                return false;
            }
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                var stateEntires = await _context.SaveChangesAsync();
                return stateEntires > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while save changes in database");
                return false;
            }
        }

        public bool Update(OrderItem entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _context.Entry(entity).State = EntityState.Modified;
            return Save();
        }

        public async Task<bool> UpdateAsync(OrderItem entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _context.Entry(entity).State = EntityState.Modified;
            return await SaveAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

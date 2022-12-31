using ElasticsearchExample.Models;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchExample.Data.Repos
{
    public class CustomersRepository : ICustomersRepository
    {
        private readonly ILogger<CustomersRepository> _logger;
        private readonly AppDbContext _context;

        public CustomersRepository(
            ILogger<CustomersRepository> logger,
            AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void Create(Customer entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _context.Customers.Add(entity);
        }

        public async Task CreateAsync(Customer entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Customers.AddAsync(entity);
        }

        public void Delete(Customer entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _context.Customers.Remove(entity);
        }

        public IEnumerable<Customer> GetAll()
        {
            return _context.Customers;
        }

        public Customer? GetById(Guid id)
        {
            return _context.Customers.FirstOrDefault(c => c.Id == id);
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

        public bool Update(Customer entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _context.Entry(entity).State = EntityState.Modified;
            return Save();
        }

        public async Task<bool> UpdateAsync(Customer entity)
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

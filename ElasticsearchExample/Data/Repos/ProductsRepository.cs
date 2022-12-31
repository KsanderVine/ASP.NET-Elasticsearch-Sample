using ElasticsearchExample.Models;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchExample.Data.Repos
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly ILogger<ProductsRepository> _logger;
        private readonly AppDbContext _context;

        public ProductsRepository(
            ILogger<ProductsRepository> logger,
            AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void Create(Product entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            throw new NotImplementedException();
        }

        public Task CreateAsync(Product entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            throw new NotImplementedException();
        }

        public void Delete(Product entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetAll()
        {
            return _context.Products;
        }

        public Product? GetById(Guid id)
        {
            return _context.Products.FirstOrDefault(p => p.Id == id);
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

        public bool Update(Product entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _context.Entry(entity).State = EntityState.Modified;
            return Save();
        }

        public async Task<bool> UpdateAsync(Product entity)
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

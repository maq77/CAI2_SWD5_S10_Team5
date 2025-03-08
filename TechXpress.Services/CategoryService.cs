using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;


public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CategoryService> _logger;
    private const string CacheKey = "CategoryList";
    public CategoryService(IUnitOfWork unitOfWork, IMemoryCache cache, ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDTO>> GetAllCategories()
    {
        if (!_cache.TryGetValue(CacheKey, out List<CategoryDTO>? categories))
        {
            _logger.LogInformation("Cache miss: Fetching categories from database.");
            var categoryEntities = await _unitOfWork.Categories.GetAll();
            categories = categoryEntities.Select(c => new CategoryDTO { Id = c.Id, Name = c.Name }).ToList();

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            _cache.Set(CacheKey, categories, cacheOptions);
        }
        else
        {
            _logger.LogInformation("Cache hit: Returning categories from cache.");
        }

        return categories!;
    }

    public async Task<CategoryDTO?> GetCategoryById(int id)
    {
        _logger.LogInformation("Fetching category with ID: {CategoryId} from database.", id);
        var category = await _unitOfWork.Categories.GetById(id);
        return category == null ? null : new CategoryDTO { Id = category.Id, Name = category.Name };
    }

    public async Task<bool> AddCategory(CategoryDTO model)
    {
        var category = new Category { Name = model.Name };
        await _unitOfWork.Categories.Add(category, log=>Console.WriteLine(log));
        bool result = await _unitOfWork.SaveAsync();
        if (result)
        {
            _cache.Remove(CacheKey); // Clear cache after adding
            _logger.LogInformation("Category '{CategoryName}' added successfully!", model.Name);
        }

        return result;
    }

    public async Task<bool> UpdateCategory(CategoryDTO model)
    {
        var category = await _unitOfWork.Categories.GetById(model.Id);
        if (category == null) return false;

        category.Name = model.Name;
        await _unitOfWork.Categories.Update(category, log => _logger.LogInformation(log));
        bool result = await _unitOfWork.SaveAsync();
        if (result)
        {
            _cache.Remove(CacheKey); // Clear cache after updating
            _logger.LogInformation("Category ID: {CategoryId} updated successfully!", model.Id);
        }

        return result;
    }

    public async Task<bool> DeleteCategory(int id)
    {
        await _unitOfWork.Categories.Delete(id, log => _logger.LogInformation(log));
        bool result = await _unitOfWork.SaveAsync();
        if (result)
        {
            _cache.Remove(CacheKey); // Clear cache after deleting
            _logger.LogInformation("Category ID: {CategoryId} deleted successfully!", id);
        }

        return result;
    }
}

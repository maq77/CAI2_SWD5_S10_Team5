using TechXpress.Data.Model;
using TechXpress.Data.Repositories;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;


public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CategoryDTO>> GetAllCategories()
    {
        var categories = await _unitOfWork.Categories.GetAll();
        return categories.Select(c => new CategoryDTO
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();
    }

    public async Task<CategoryDTO?> GetCategoryById(int id)
    {
        var category = await _unitOfWork.Categories.GetById(id);
        return category == null ? null : new CategoryDTO { Id = category.Id, Name = category.Name };
    }

    public async Task<bool> AddCategory(CategoryDTO model)
    {
        var category = new Category { Name = model.Name };
        await _unitOfWork.Categories.Add(category, log=>Console.WriteLine(log));
        return await _unitOfWork.SaveAsync();
    }

    public async Task<bool> UpdateCategory(CategoryDTO model)
    {
        var category = await _unitOfWork.Categories.GetById(model.Id);
        if (category == null) return false;

        category.Name = model.Name;
        await _unitOfWork.Categories.Update(category, log => Console.WriteLine(log));
        return await _unitOfWork.SaveAsync();
    }

    public async Task<bool> DeleteCategory(int id)
    {
        await _unitOfWork.Categories.Delete(id, log => Console.WriteLine(log));
        return await _unitOfWork.SaveAsync();
    }
}

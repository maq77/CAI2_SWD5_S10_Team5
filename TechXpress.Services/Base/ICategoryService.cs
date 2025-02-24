using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategories();
        Task<CategoryDTO?> GetCategoryById(int id);
        Task<bool> AddCategory(CategoryDTO model);
        Task<bool> UpdateCategory(CategoryDTO model);
        Task<bool> DeleteCategory(int id);
    }
}

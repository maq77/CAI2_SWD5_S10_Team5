using System.Collections.Generic;
using X.PagedList;

namespace TechXpress.Services.DTOs
{
    public class HomePageDTO
    {
        public IEnumerable<ProductDTO> FeaturedProducts { get; set; } = new List<ProductDTO>();
        public IEnumerable<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
        public IPagedList<ProductDTO>? PaginatedProducts { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace TechXpress.Services.DTOs
{
    public class ShopPageDTO
    {
        public IPagedList<ProductDTO> Products { get; set; }
        public IEnumerable<CategoryDTO> Categories { get; set; }
        public string SearchQuery { get; set; }
        public int? SelectedCategoryId { get; set; }
    }
}

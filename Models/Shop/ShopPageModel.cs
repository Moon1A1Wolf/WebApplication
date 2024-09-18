using WebApplication1.Data.Entities;

namespace WebApplication1.Models.Shop
{
    public class ShopPageModel
    {
        public IEnumerable<ProductGroup> ProductGroups { get; set; }
    }
}

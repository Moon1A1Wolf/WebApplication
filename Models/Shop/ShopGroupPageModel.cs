using WebApplication1.Data.Entities;

namespace WebApplication1.Models.Shop
{
    public class ShopGroupPageModel
    {
        public ProductGroup ProductGroup { get; set; } = null!;
        public IEnumerable<ProductGroup> Groups { get; set; } = null!;
    }
}

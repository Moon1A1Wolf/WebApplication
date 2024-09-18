using WebApplication1.Data.Entities;

namespace WebApplication1.Models.Shop
{
    public class ShopProductPageModel
    {
        public Product Product { get; set; } = null!;
        public ProductGroup ProductGroup { get; set; } = null!;
    }
}

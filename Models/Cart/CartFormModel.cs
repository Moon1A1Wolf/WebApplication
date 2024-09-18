namespace WebApplication1.Models.Cart
{
    public class CartFormModel
    {
        public Guid ProductId { get; set; }
        public Guid UserId    { get; set; }
        public int  Cnt       { get; set; }
    }
}

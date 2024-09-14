using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Home
{
    public class Product
    {
        [Required(ErrorMessage = "Назва є обов'язковою")]
        [StringLength(100, ErrorMessage = "Назва товару не може перевищувати 100 символів")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Опис є обов'язковим")]
        [StringLength(500, ErrorMessage = "Опис товару не може перевищувати 500 символів")]
        public string Description { get; set; } = "esgs";

        [Required(ErrorMessage = "Ціна є обов'язковою")]
        [RegularExpression(@"^(?!0|(0(\.|,)?0{1,2})$)\d+((\.|,)?\d{1,2})?$", ErrorMessage = "Ціна повинна бути числом більше нуля та мати до двох знаків після коми")]
        public string Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Кількість на складі повинна бути невід'ємною")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Потрібно додати зображення")]
        public IFormFile ProductImg { get; set; }
    }
}

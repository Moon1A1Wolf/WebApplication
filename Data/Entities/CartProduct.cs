﻿using System.Text.Json.Serialization;

namespace WebApplication1.Data.Entities
{
    public class CartProduct
    {
        public Guid Id         { get; set; }
        public Guid CartId     { get; set; }
        public Guid ProductId  { get; set; }
        public int  Cnt        { get; set; } = 1;

        [JsonIgnore]
        public Cart    Cart    { get; set; }
        public Product Product { get; set; }
    }
}

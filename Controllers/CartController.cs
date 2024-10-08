﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;
using WebApplication1.Models.Api;
using WebApplication1.Models.Cart;

namespace WebApplication1.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController(DataContext dataContext) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;

        [HttpGet]
        public RestResponse<Cart?> DoGet([FromQuery] String id)
        {
            RestResponse<Cart?> response = new()
            {
                Meta = new()
                {
                    Service = "Cart",
                },
                Data = _dataContext
                        .Carts
                        .Include(c => c.CartProducts)
                            .ThenInclude(cp => cp.Product)
                        .FirstOrDefault(c =>
                            c.UserId.ToString() == id &&
                            c.CloseDt == null &&
                            c.DeleteDt == null)
            };

            return response;
        }

        [HttpPost]
        public async Task<RestResponse<String>> DoPost(
                            [FromBody] CartFormModel formModel)
        {
            RestResponse<String> response = new()
            {
                Meta = new()
                {
                    Service = "Cart",
                },
            };
            if (formModel.UserId == default)
            {
                response.Data = "Error 401: Unauthorized";
                return response;
            }
            if (formModel.ProductId == default)
            {
                response.Data = "Error 422: Missing Product Id";
                return response;
            }
            if (formModel.Cnt <= 0)
            {
                response.Data = "Error 422: Positive Cnt expected";
                return response;
            }
            // Чи є у користувача відкритий кошик? Якщо є, то додаємо
            // товари до нього, якщо немає, то створюємо і додаємо до нового
            var cart = _dataContext
                .Carts
                .FirstOrDefault(c =>
                    c.UserId == formModel.UserId &&
                    c.CloseDt == null &&
                    c.DeleteDt == null);

            if (cart == null)   // немає відкритого кошику, треба створювати
            {
                Guid cartId = Guid.NewGuid();
                _dataContext.Carts.Add(new()
                {
                    Id = cartId,
                    UserId = formModel.UserId,
                    CreateDt = DateTime.Now,
                });
                _dataContext.CartProducts.Add(new()
                {
                    Id = Guid.NewGuid(),
                    CartId = cartId,
                    ProductId = formModel.ProductId,
                    Cnt = formModel.Cnt,
                });
            }
            else   // є відкритий кошик, треба додавати до нього
            {
                // треба перевірити, чи є вже такий товар у кошику,
                // якщо є, то збільшити кількість, якщо немає, то додати
                var cartProduct = _dataContext
                    .CartProducts
                    .FirstOrDefault(cp =>
                        cp.CartId == cart.Id &&
                        cp.ProductId == formModel.ProductId);

                if (cartProduct == null)   // такого товару немає в кошику
                {
                    _dataContext.CartProducts.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        CartId = cart.Id,
                        ProductId = formModel.ProductId,
                        Cnt = formModel.Cnt,
                    });
                }
                else  // такий товар є в кошику
                {
                    cartProduct.Cnt += formModel.Cnt;
                }
            }
            await _dataContext.SaveChangesAsync();
            response.Data = "Added";
            return response;
        }

        [HttpPut]
        public async Task<RestResponse<String>> DoPut(
            [FromQuery] Guid cpId, [FromQuery] int increment)
        {
            RestResponse<String> response = new()
            {
                Meta = new()
                {
                    Service = "Cart",
                },
            };
            if (cpId == default)
            {
                response.Data = "Error 400: cpId is not valid";
                return response;
            }
            if (increment == 0)
            {
                response.Data = "Error 400: increment is not valid";
                return response;
            }
            var cp = _dataContext
                .CartProducts
                .Include(cp => cp.Cart)
                .FirstOrDefault(cp => cp.Id == cpId);
            if (cp == null)
            {
                response.Data = "Error 404: cpId does not identify entity";
                return response;
            }
            if (cp.Cart.CloseDt is not null || cp.Cart.DeleteDt is not null)
            {
                response.Data = "Error 409: cpId identifies not active entity";
                return response;
            }
            if (cp.Cnt + increment < 0)
            {
                response.Data = "Error 422: increment could not be applied";
                return response;
            }

            if (cp.Cnt + increment == 0)
            {
                // віднімання усього -- видалення 
                _dataContext.CartProducts.Remove(cp);
                response.Meta.Count = 0;
            }
            else
            {
                // оновлення кількості
                cp.Cnt += increment;
                response.Meta.Count = cp.Cnt;
            }
            await _dataContext.SaveChangesAsync();
            response.Data = "Updated";
            return response;
        }
    }
}
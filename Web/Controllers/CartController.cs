using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models.Data;
using Web.Models.ViewModels.Cart;

namespace Web.Controllers
{
    public class CartController : Controller
    {
        public ActionResult Index()
        {
            var cart = (List<CartViewModel>)Session["cart"] ?? new List<CartViewModel>();

            if (!cart.Any())
            {
                ViewBag.CartMessage = "Your cart is empty.";
                return View();
            }

            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Price * item.Quantity;
            }

            ViewBag.CartTotal = total;

            return View(cart);
        }

        public PartialViewResult CartPartial()
        {
            CartViewModel cart = new CartViewModel
            {
                Quantity = 0,
                Price = 0m
            };

            if (Session["cart"] != null)
            {
                var list = (List<CartViewModel>)Session["cart"];

                foreach (var item in list)
                {
                    cart.Quantity += item.Quantity;
                    cart.Price += item.Quantity * item.Price;
                }
            }

            return PartialView(cart);
        }

        public PartialViewResult AddToCartPartial(int id)
        {
            var cart = (List<CartViewModel>)Session["cart"] ?? new List<CartViewModel>();
            CartViewModel cartResult = new CartViewModel();
            
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                var productInCart = cart.FirstOrDefault(x => x.ProductId == dto.Id);

                if (productInCart == null)
                {
                    cart.Add(new CartViewModel
                    {
                        ProductId = dto.Id,
                        ProductName = dto.Name,
                        Quantity = 1,
                        Price = dto.Price,
                        Image = dto.ImageName,
                    });
                }
                else
                {
                    productInCart.Quantity++;
                }
            }

            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Price * item.Quantity;
            }

            cartResult.Quantity = qty;
            cartResult.Price = price;

            Session["cart"] = cart;

            return PartialView(cartResult);
        }

        public JsonResult IncrementProduct(int productId)
        {
            var cart = (List<CartViewModel>)Session["cart"] ;

            CartViewModel cartItem = cart.FirstOrDefault(x => x.ProductId == productId);
            cartItem.Quantity++;

            var result = new { qty = cartItem.Quantity, price = cartItem.Price };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DecrementProduct(int productId)
        {
            var cart = (List<CartViewModel>)Session["cart"];

            CartViewModel cartItem = cart.FirstOrDefault(x => x.ProductId == productId);

            cartItem.Quantity--;
            if (cartItem.Quantity <= 0)
            {
                cartItem.Quantity = 0;
                cart.Remove(cartItem);
            }

            var result = new { qty = cartItem.Quantity, price = cartItem.Price };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public void RemoveProduct(int productId)
        {
            var cart = (List<CartViewModel>)Session["cart"];

            CartViewModel cartItem = cart.FirstOrDefault(x => x.ProductId == productId);
            cart.Remove(cartItem);
        }

        public PartialViewResult PaypalPartial()
        {
            var cart = (List<CartViewModel>)Session["cart"];

            return PartialView(cart);
        }
    }
}
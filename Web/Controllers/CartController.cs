using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using Web.Infrastructure;
using Web.Models.Data;
using Web.Models.ViewModels.Cart;

namespace Web.Controllers
{
    public class CartController : Controller
    {
        [Authorize(Roles = "User")]
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

        [HttpPost]
        public void PlaceOrder()
        {
            var cart = (List<CartViewModel>)Session["cart"];
            string username = User.Identity.Name;
            int orderId;

            using (Db db = new Db())
            {
                int userId = db.Users.FirstOrDefault(x => x.Username == username).Id;

                OrderDTO orderDto = new OrderDTO
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };

                db.Orders.Add(orderDto);
                db.SaveChanges();

                orderId = orderDto.OrderId;

                //Add the details for this item + orderId for each item in the cart
                foreach (var item in cart)
                {
                    OrderDetailsDTO orderDetailsDto = new OrderDetailsDTO
                    {
                        OrderId = orderId,
                        UserId = userId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    };

                    db.OrderDetails.Add(orderDetailsDto);
                }

                db.SaveChanges();
            }

            SendEmail(orderId);

            Session["cart"] = null;
        }

        private void SendEmail(int orderId)
        {
            EmailSettings emailSettings = new EmailSettings();
            emailSettings.ReadFromFile(AppDomain.CurrentDomain.BaseDirectory + "App_Data/secret_settings.txt");
            //Email to admin
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.EnableSsl = emailSettings.UseSsl;
                smtpClient.Host = emailSettings.ServerName;
                smtpClient.Port = emailSettings.ServerPort;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password);

                if (emailSettings.WriteAsFile)
                {
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtpClient.PickupDirectoryLocation = emailSettings.FileLocation;
                    smtpClient.EnableSsl = false;
                }

                string body = $"New order Id: {orderId}";

                MailMessage mailMessage = new MailMessage(emailSettings.MailFromAddress, emailSettings.MailToAddress, "New order submitted!", body);

                if (emailSettings.WriteAsFile)
                {
                    mailMessage.BodyEncoding = Encoding.ASCII;
                }

                smtpClient.Send(mailMessage);
            }
        }
    }
}
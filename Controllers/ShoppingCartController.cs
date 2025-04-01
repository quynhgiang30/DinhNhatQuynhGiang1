using DinhNhatQuynhGiang1.Models;
using DinhNhatQuynhGiang1.Extensions;
using DinhNhatQuynhGiang1.Models.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DinhNhatQuynhGiang1.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(
            IProductRepository productRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ShoppingCartController> logger)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await GetUserCartAsync();
            return View(cart);
        }

        public IActionResult Checkout()
        {
            return View(new Order());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Ensure user is logged in
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = await GetUserCartAsync();

            if (cart == null || !cart.Items.Any())
            {
                _logger.LogWarning("Checkout attempted with empty cart");
                TempData["ErrorMessage"] = "Your cart is empty";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("Checkout attempted without user authentication");
                return RedirectToAction("Login", "Account");
            }

            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Order {order.Id} created successfully for user {user.Id}");

                // Clear the user's cart after successful checkout
                await ClearUserCartAsync();
                return View("OrderCompleted", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating order for user {user.Id}");
                TempData["ErrorMessage"] = "There was an error processing your order. Please try again.";
                return View(order);
            }
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            try
            {
                var product = await GetProductFromDatabase(productId);

                if (product == null)
                {
                    _logger.LogWarning($"Attempt to add non-existent product {productId} to cart");
                    TempData["ErrorMessage"] = "Product not found";
                    return RedirectToAction("Index");
                }

                var cartItem = new CartItem
                {
                    ProductId = productId,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                };

                var cart = await GetUserCartAsync();
                cart.AddItem(cartItem);
                await SaveUserCartAsync(cart);

                _logger.LogInformation($"Product {productId} ({product.Name}) added to cart");
                TempData["SuccessMessage"] = $"{product.Name} added to cart";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product {productId} to cart");
                TempData["ErrorMessage"] = "Error adding product to cart";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                var cart = await GetUserCartAsync();

                if (cart == null || !cart.Items.Any())
                {
                    _logger.LogWarning("Attempt to remove from empty cart");
                    TempData["ErrorMessage"] = "Your cart is empty";
                    return RedirectToAction("Index");
                }

                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

                if (item == null)
                {
                    _logger.LogWarning($"Attempt to remove non-existent product {productId} from cart");
                    TempData["ErrorMessage"] = "Product not found in cart";
                    return RedirectToAction("Index");
                }

                cart.RemoveItem(productId);
                await SaveUserCartAsync(cart);

                _logger.LogInformation($"Product {productId} ({item.Name}) removed from cart");
                TempData["SuccessMessage"] = $"{item.Name} removed from cart";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing product {productId} from cart");
                TempData["ErrorMessage"] = "Error removing product from cart";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                await ClearUserCartAsync();
                _logger.LogInformation("Cart cleared");
                TempData["SuccessMessage"] = "Your cart has been cleared";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                TempData["ErrorMessage"] = "Error clearing cart";
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        public async Task<IActionResult> OrderCompleted(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogWarning($"Unauthorized access attempt to order {id}");
                    return RedirectToAction("Login", "Account");
                }

                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

                if (order == null)
                {
                    _logger.LogWarning($"Order {id} not found for user {user.Id}");
                    return NotFound();
                }

                _logger.LogInformation($"Order {id} details viewed by user {user.Id}");
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving order {id}");
                TempData["ErrorMessage"] = "Error retrieving order details";
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task<Product> GetProductFromDatabase(int productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }

        // New helper methods for user-specific cart management
        private async Task<ShoppingCart> GetUserCartAsync()
        {
            string cartKey = GetCartSessionKey();
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartKey);

            if (cart == null)
            {
                cart = new ShoppingCart();

                // For authenticated users, we could also retrieve their saved cart from database
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        // Optional: You could load a persisted cart from database here
                        // cart = await _cartRepository.GetCartForUserAsync(user.Id) ?? new ShoppingCart();
                        _logger.LogInformation($"New cart created for user {user.Id}");
                    }
                }
            }

            return cart;
        }

        private async Task SaveUserCartAsync(ShoppingCart cart)
        {
            string cartKey = GetCartSessionKey();
            HttpContext.Session.SetObjectAsJson(cartKey, cart);

            // For authenticated users, we could also save their cart to database
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Optional: You could save the cart to database here
                    // await _cartRepository.SaveCartForUserAsync(user.Id, cart);
                    _logger.LogInformation($"Cart updated for user {user.Id} with {cart.Items.Count} items");
                }
            }
        }

        private async Task ClearUserCartAsync()
        {
            string cartKey = GetCartSessionKey();
            HttpContext.Session.Remove(cartKey);

            // For authenticated users, we could also clear their cart from database
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Optional: You could clear the cart from database here
                    // await _cartRepository.ClearCartForUserAsync(user.Id);
                    _logger.LogInformation($"Cart cleared for user {user.Id}");
                }
            }
        }

        private string GetCartSessionKey()
        {
            // For authenticated users, use their ID as part of the cart key
            if (User.Identity.IsAuthenticated)
            {
                return $"Cart_{User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value}";
            }

            // For anonymous users, use a session ID or similar
            string sessionId = HttpContext.Session.Id;
            return $"Cart_{sessionId}";
        }
    }
}
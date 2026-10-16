using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication2.Pages.Cart
{
    public class CartLine
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
    }

    public class IndexModel : PageModel
    {
        // Товары, сгруппированные по ID (одна строка = один товар + количество)
        public List<CartLine> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }

        [TempData] public string? FlashMessage { get; set; }
        [TempData] public string? FlashType { get; set; }

        public void OnGet()
        {
            LoadCart();

            if (string.IsNullOrEmpty(FlashMessage) && Items.Count == 0)
            {
                FlashMessage = "Корзина пуста";
                FlashType = "warning";
            }
        }

        public IActionResult OnPostClear()
        {
            HttpContext.Session.Remove("Cart");

            FlashMessage = "Корзина очищена";
            FlashType = "warning";
            return RedirectToPage();
        }

        public IActionResult OnPostConfirm()
        {
            HttpContext.Session.Remove("Cart");

            FlashMessage = "Заказ успешно оформлен! Спасибо за покупку.";
            FlashType = "success";
            return RedirectToPage();
        }

        // ---- Общий метод чтения корзины из сессии ----
        private void LoadCart()
        {
            var raw = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(raw))
            {
                Items = new();
                TotalAmount = 0;
                return;
            }

            var ids = raw
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            // Группируем по ID → сколько раз встречается
            Items = ids
                .GroupBy(id => id)
                .Select(g =>
                {
                    var product = ProductCatalog.Find(g.Key);
                    return new CartLine
                    {
                        ProductId = g.Key,
                        Name = product?.Name ?? "Неизвестный товар",
                        Price = product?.Price ?? 0,
                        Quantity = g.Count()
                    };
                })
                .ToList();

            TotalAmount = Items.Sum(i => i.Subtotal);
        }
    }
}
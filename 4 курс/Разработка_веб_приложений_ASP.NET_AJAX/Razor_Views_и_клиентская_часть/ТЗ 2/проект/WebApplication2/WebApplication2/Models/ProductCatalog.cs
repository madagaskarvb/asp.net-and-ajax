namespace WebApplication2
{
    public class CatalogItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string ImageUrl { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public int Stock { get; set; } = 10;
        public bool IsHit { get; set; } = false;

        public bool HasDiscount => OldPrice.HasValue && OldPrice.Value > Price;
        public int DiscountPercent => HasDiscount
            ? (int)Math.Round((1 - Price / OldPrice!.Value) * 100)
            : 0;

        public bool IsOutOfStock => Stock == 0;
        public bool IsBigDiscount => DiscountPercent > 30;
    }

    public class ProductReview
    {
        public string Author { get; set; } = "";
        public string Text { get; set; } = "";
        public int Rating { get; set; }   // 1..5
    }

    public static class ProductCatalog
    {
        public const int PageSize = 6;   // сколько товаров на одной "странице"

        public static readonly List<string> AllTags = new()
        {
            "Электроника", "Аксессуары", "Хиты"
        };

        public static readonly List<CatalogItem> All = BuildCatalog();

        public static CatalogItem? Find(int id) => All.FirstOrDefault(p => p.Id == id);

        public static List<CatalogItem> FilterByTag(string? tag)
        {
            if (string.IsNullOrWhiteSpace(tag) || tag == "Все")
                return All;
            return All.Where(p => p.Tags.Contains(tag)).ToList();
        }

        // ---- Постраничная выборка для infinite scroll ----
        public static List<CatalogItem> GetPage(int page, int pageSize = PageSize)
        {
            if (page < 1) page = 1;
            return All.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        // ================ Отзывы (мок-данные) ================
        // Генерим детерминированно по ID товара:
        // у чётных — отзывы есть, у нечётных — пусто (для демонстрации)
        public static List<ProductReview> GetReviews(int productId)
        {
            var product = Find(productId);
            if (product == null) return new List<ProductReview>();

            if (productId % 2 != 0)
                return new List<ProductReview>();

            var authors = new[] { "Анна К.", "Дмитрий П.", "Сергей В.", "Ольга М.", "Иван Л." };
            var texts = new[]
            {
                "Отличный товар, полностью оправдал ожидания!",
                "Хорошее качество за свои деньги. Рекомендую.",
                "Быстрая доставка, всё работает как надо.",
                "Нормально, но есть небольшие недочёты.",
                "Пользуюсь месяц — впечатления только положительные."
            };
            var rnd = new Random(productId);   // детерминированный «рандом» по ID
            var count = rnd.Next(2, 5);

            var list = new List<ProductReview>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new ProductReview
                {
                    Author = authors[rnd.Next(authors.Length)],
                    Text = texts[rnd.Next(texts.Length)],
                    Rating = rnd.Next(3, 6)   // 3..5
                });
            }
            return list;
        }

        // ---- Строим каталог из 6 базовых + 18 клонов = 24 товара ----
        private static List<CatalogItem> BuildCatalog()
        {
            var baseItems = new List<CatalogItem>
            {
                new CatalogItem
                {
                    Id = 1, Name = "Наушники Sony WH-1000XM5",
                    Description = "Беспроводные наушники с активным шумоподавлением и до 30 часов работы.",
                    Price = 34990m, OldPrice = 39990m,
                    ImageUrl = "https://placehold.co/400x300/0d6efd/ffffff?text=Headphones",
                    Tags = new() { "Электроника", "Хиты" },
                    Stock = 12, IsHit = true
                },
                new CatalogItem
                {
                    Id = 2, Name = "Смартфон Galaxy S24",
                    Description = "Флагман с AMOLED-экраном 120 Гц, тройной камерой и быстрой зарядкой.",
                    Price = 59990m, OldPrice = 89990m,
                    ImageUrl = "https://placehold.co/400x300/198754/ffffff?text=Smartphone",
                    Tags = new() { "Электроника" },
                    Stock = 5
                },
                new CatalogItem
                {
                    Id = 3, Name = "Ноутбук MacBook Air M3",
                    Description = "Лёгкий, тихий, с автономностью до 18 часов. Идеален для работы и учёбы.",
                    Price = 129990m, OldPrice = null,
                    ImageUrl = "https://placehold.co/400x300/6c757d/ffffff?text=Laptop",
                    Tags = new() { "Электроника" },
                    Stock = 3
                },
                new CatalogItem
                {
                    Id = 4, Name = "Умные часы Apple Watch",
                    Description = "Отслеживание активности, ЭКГ, Always-On дисплей и влагозащита.",
                    Price = 39990m, OldPrice = 44990m,
                    ImageUrl = "https://placehold.co/400x300/212529/ffffff?text=Watch",
                    Tags = new() { "Электроника", "Хиты" },
                    Stock = 0, IsHit = true
                },
                new CatalogItem
                {
                    Id = 5, Name = "Механическая клавиатура Keychron K2",
                    Description = "Компактная 75%, беспроводная, с hot-swap переключателями.",
                    Price = 8990m, OldPrice = 9990m,
                    ImageUrl = "https://placehold.co/400x300/dc3545/ffffff?text=Keyboard",
                    Tags = new() { "Аксессуары" },
                    Stock = 8
                },
                new CatalogItem
                {
                    Id = 6, Name = "Мышь Logitech MX Master 3S",
                    Description = "Эргономичная мышь с бесшумными кликами и высокой точностью 8K DPI.",
                    Price = 10990m, OldPrice = null,
                    ImageUrl = "https://placehold.co/400x300/0dcaf0/000000?text=Mouse",
                    Tags = new() { "Аксессуары" },
                    Stock = 20
                }
            };

            var all = new List<CatalogItem>(baseItems);

            // Клонируем базовые товары 3 раза с суффиксами — получаем 24 товара
            string[] suffixes = { "Pro", "Max", "Lite" };
            foreach (var suffix in suffixes)
            {
                foreach (var src in baseItems)
                {
                    var nextId = all.Max(x => x.Id) + 1;
                    all.Add(Clone(src, nextId, suffix));
                }
            }

            return all;
        }

        private static CatalogItem Clone(CatalogItem src, int newId, string suffix)
        {
            return new CatalogItem
            {
                Id = newId,
                Name = src.Name + " " + suffix,
                Description = src.Description,
                Price = src.Price,
                OldPrice = src.OldPrice,
                ImageUrl = src.ImageUrl,
                Tags = new List<string>(src.Tags),
                Stock = src.Stock,
                IsHit = src.IsHit
            };
        }
    }
}
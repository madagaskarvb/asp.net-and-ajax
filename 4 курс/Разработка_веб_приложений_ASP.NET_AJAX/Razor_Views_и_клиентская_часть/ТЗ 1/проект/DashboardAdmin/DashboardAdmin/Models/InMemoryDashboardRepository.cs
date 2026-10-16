using DashboardAdmin.Models;
using System.Collections.Generic;
using System.Linq;

namespace DashboardAdmin.Models
{
    public class InMemoryDashboardRepository : IDashboardRepository
    {
        private static readonly List<DashboardCard> _cards = new()
        {
            new DashboardCard
            {
                Id = 1,
                Title = "Выручка за месяц",
                Value = 1_250_000m,
                Trend = Trend.Up,
                Unit = "руб.",
                Description = "Общая выручка за текущий месяц"
            },
            new DashboardCard
            {
                Id = 2,
                Title = "Количество заказов",
                Value = 342m,
                Trend = Trend.Up,
                Unit = "шт.",
                Description = "Число оформленных заказов"
            },
            new DashboardCard
            {
                Id = 3,
                Title = "Средний чек",
                Value = 3_654m,
                Trend = Trend.Stable,
                Unit = "руб.",
                Description = "Средняя стоимость одного заказа"
            },
            new DashboardCard
            {
                Id = 4,
                Title = "Возвраты",
                Value = -12_500m,
                Trend = Trend.Down,
                Unit = "руб.",
                Description = "Сумма возвратов за период (отрицательное значение)"
            },
            new DashboardCard
            {
                Id = 5,
                Title = "Конверсия",
                Value = 4.7m,
                Trend = Trend.Up,
                Unit = "%",
                Description = "Процент посетителей, оформивших заказ"
            },
            new DashboardCard
            {
                Id = 6,
                Title = "Активные клиенты",
                Value = 128m,
                Trend = Trend.Stable,
                Unit = "шт.",
                Description = "Клиенты с покупками за последние 30 дней"
            }
        };

        public IEnumerable<DashboardCard> GetAllCards() => _cards;

        public DashboardCard? GetById(int id) =>
            _cards.FirstOrDefault(c => c.Id == id);

        public IEnumerable<DashboardCard> GetLatest(int count) =>
            _cards.OrderByDescending(c => c.Id).Take(count);
    }
}
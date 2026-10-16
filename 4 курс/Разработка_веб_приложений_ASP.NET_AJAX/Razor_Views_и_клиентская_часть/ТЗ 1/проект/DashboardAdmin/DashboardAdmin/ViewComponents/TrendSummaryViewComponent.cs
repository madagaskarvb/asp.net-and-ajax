using System.Collections.Generic;
using System.Linq;
using DashboardAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DashboardAdmin.ViewComponents
{
    public class TrendSummaryViewComponent : ViewComponent
    {
        private readonly IDashboardRepository _repo;

        // DI — получаем репозиторий
        public TrendSummaryViewComponent(IDashboardRepository repo)
        {
            _repo = repo;
        }

        public IViewComponentResult Invoke()
        {
            var cards = _repo.GetAllCards().ToList();

            // Группируем по тренду: количество + сумма значений
            var summary = cards
                .GroupBy(c => c.Trend)
                .Select(g => new TrendSummaryModel
                {
                    Trend = g.Key,
                    Label = GetLabel(g.Key),
                    CssClass = GetCssClass(g.Key),
                    Count = g.Count(),
                    Total = g.Sum(x => x.Value)
                })
                // фиксированный порядок: Up, Down, Stable
                .OrderBy(s => s.Trend)
                .ToList();

            // Возвращаем View(model) — список сводок
            return View(summary);
        }

        private static string GetLabel(Trend t) => t switch
        {
            Trend.Up => "Рост",
            Trend.Down => "Снижение",
            Trend.Stable => "Стабильно",
            _ => "—"
        };

        private static string GetCssClass(Trend t) => t switch
        {
            Trend.Up => "up",
            Trend.Down => "down",
            Trend.Stable => "stable",
            _ => ""
        };
    }
}
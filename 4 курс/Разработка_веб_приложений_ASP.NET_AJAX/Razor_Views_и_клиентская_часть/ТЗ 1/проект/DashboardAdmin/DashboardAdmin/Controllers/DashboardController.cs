using DashboardAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DashboardAdmin.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _repo;

        public DashboardController(IDashboardRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            ViewData["ReportPeriod"] = "Сентябрь 2026";
            ViewBag.Currency = "RUB";
            var cards = _repo.GetAllCards();
            return View(cards);
        }

        // 🔹 Новое действие для печати
        public IActionResult Print()
        {
            ViewData["Mode"] = "Print";  // Переключаем layout на _PrintLayout
            ViewData["ReportPeriod"] = "Сентябрь 2026";
            ViewBag.Currency = "RUB";
            var cards = _repo.GetAllCards();
            return View("Index", cards); // Возвращаем ту же вьюху Index.cshtml
        }

        // 🔹 Добавим Details, чтобы показать отсутствие секции Actions
        public IActionResult Details(int id)
        {
            var card = _repo.GetById(id);
            if (card == null) return NotFound();
            return View(card);
        }
    }
}
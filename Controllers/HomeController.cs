// File: Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicketRadar.Models;
using TicketRadar.Services;

namespace TicketRadar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TicketCheckerService _ticketCheckerService;

        public HomeController(ILogger<HomeController> logger, TicketCheckerService ticketCheckerService)
        {
            _logger = logger;
            _ticketCheckerService = ticketCheckerService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.SelectedTeam = TicketStatusStore.SelectedTeam;
            ViewBag.LastStatus = TicketStatusStore.LastStatus;
            ViewBag.LastCheckTime = TicketStatusStore.LastCheckTime?.ToString("yyyy/MM/dd hh:mm:ss tt");
            ViewBag.BookingUrl = TicketStatusStore.LastBookingUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartRadar(string teamName)
        {
            var selectedTeam = teamName?.Trim() ?? "";

            TicketStatusStore.SelectedTeam = selectedTeam;
            TicketStatusStore.LastStatus = string.IsNullOrWhiteSpace(selectedTeam)
                ? "اختر النادي واضغط تشغيل الرادار"
                : $"الرادار يعمل الآن على: {selectedTeam}";
            TicketStatusStore.LastCheckTime = DateTime.Now;
            TicketStatusStore.LastBookingUrl = "https://www.tazkarti.com/#/matches";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Status(CancellationToken cancellationToken)
        {
            await _ticketCheckerService.CheckAsync(cancellationToken);

            return Json(new
            {
                selectedTeam = TicketStatusStore.SelectedTeam,
                lastStatus = TicketStatusStore.LastStatus,
                lastCheckTime = TicketStatusStore.LastCheckTime?.ToString("yyyy/MM/dd hh:mm:ss tt"),
                bookingUrl = TicketStatusStore.LastBookingUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StopRadar()
        {
            TicketStatusStore.SelectedTeam = "";
            TicketStatusStore.LastStatus = "تم إيقاف الرادار";
            TicketStatusStore.LastCheckTime = DateTime.Now;
            TicketStatusStore.LastBookingUrl = "https://www.tazkarti.com/#/matches";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
using FinanceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FinanceManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("BalanceReports")]
        public IActionResult BalanceReports()
        {
            return View();
        }

        [Route("Transfers")]
        public IActionResult Transfers()
        {
            return View();
        }

        [Route("RecurringTransfers")]
        public IActionResult RecurringTransfers()
        {
            return View();
        }

        [Route("Categories")]
        public IActionResult Categories()
        {
            return View();
        }

        [Route("Charts")]
        public IActionResult Charts()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
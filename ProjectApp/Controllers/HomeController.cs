using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectApp.DTO;
using ProjectApp.Models;
using System.Diagnostics;
using System.Net.Http;

namespace ProjectApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private HttpClient httpClient;
        private string apiKey = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            this.httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var apiUrl = $"https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code={apiKey}";
            var response = await httpClient.GetStringAsync(apiUrl);
            var employeeList = JsonConvert.DeserializeObject<List<EmployeeModel>>(response);
            var grouped = employeeList.GroupBy(x => x.EmployeeName)
                                       .Select(x => new EmployeeDTO
                                       {
                                           EmployeeName = string.IsNullOrEmpty(x.Key) ? "N/A" : x.Key,
                                           TotalHours = x.Sum(x => x.TotalHours)
                                       }).ToList();


            return View(grouped);
        }

        public IActionResult Privacy()
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
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectApp.DTO;
using ProjectApp.Models;
using SkiaSharp;
using System.Diagnostics;
using System.Drawing;
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

            var image = DrawPieChart(grouped);

            var imagePath = Path.Combine("wwwroot", "images", "chart.png");
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
            }

            ViewBag.ImagePath = imagePath;

            return View(grouped);
        }

        private SKBitmap DrawPieChart(List<EmployeeDTO> employeeDataList)
        {
            var width = 400;
            var height = 400;

            using (var surface = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                var totalHours = employeeDataList.Sum(e => e.TotalHours);

                var startAngle = 0.0f;

                var colors = new SKColor[]
                {
            SKColors.Blue,
            SKColors.Red,
            SKColors.Yellow,
            SKColors.Green,
            SKColors.Orange,
                };

                var colorIndex = 0;

                foreach (var employee in employeeDataList)
                {
                    var percentage = (float)(100.0 * employee.TotalHours / totalHours);
                    var sweepAngle = (float)(360.0 * percentage / 100);

                    using (var paint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = colors[colorIndex],
                    })
                    {
                        canvas.DrawArc(new SKRect(0, 0, width, height), startAngle, sweepAngle, true, paint);

                        var textPaint = new SKPaint
                        {
                            TextSize = 24.0f,
                            Color = SKColors.Black,
                            TextAlign = SKTextAlign.Center,
                            IsAntialias = true,
                        };

                        var textAngle = startAngle + sweepAngle / 2.0f;
                        var textRadius = width / 2.5f;

                        var x = width / 2.0f + textRadius * (float)Math.Cos(Math.PI * textAngle / 180.0);
                        var y = height / 2.0f + textRadius * (float)Math.Sin(Math.PI * textAngle / 180.0);

                        canvas.DrawText(employee.EmployeeName, x, y, textPaint);

                        var percentageTextPaint = new SKPaint
                        {
                            TextSize = 16.0f,
                            Color = SKColors.Black,
                            TextAlign = SKTextAlign.Center,
                            IsAntialias = true,
                        };

                        var percentageX = width / 2.0f + textRadius * (float)Math.Cos(Math.PI * textAngle / 180.0);
                        var percentageY = height / 2.0f + textRadius * (float)Math.Sin(Math.PI * textAngle / 180.0) + 20.0f;

                        canvas.DrawText($"{percentage:F2}%", percentageX, percentageY, percentageTextPaint);
                    }

                    startAngle += sweepAngle;

                    colorIndex = (colorIndex + 1) % colors.Length;
                }

                using (var skiaImage = surface.Snapshot())
                {
                    return SKBitmap.FromImage(skiaImage);
                }
            }
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
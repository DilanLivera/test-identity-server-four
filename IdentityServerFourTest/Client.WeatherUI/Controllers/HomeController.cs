using Client.WeatherUI.Models;
using Client.WeatherUI.Services;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.WeatherUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IIdentityServerService _identityServerService;

        public HomeController(
            IHttpClientFactory httpClientFactory,
            IIdentityServerService identityServerService)
        {
            _httpClientFactory = httpClientFactory;
            _identityServerService = identityServerService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> WeatherForecastAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var tokenResponse = await _identityServerService.GetTokenAsync("weatherapi.read");

            client.SetBearerToken(tokenResponse.AccessToken);

            const string URI = "https://localhost:5011/weatherforecast";
            var responseMessage = await client.GetAsync(
                URI, 
                HttpCompletionOption.ResponseHeadersRead);

            responseMessage.EnsureSuccessStatusCode();

            var result = await responseMessage.Content.ReadAsStringAsync();

            var weatherForecasts = JsonConvert.DeserializeObject<IEnumerable<WeatherForecast>>(result);

            return View(weatherForecasts);
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

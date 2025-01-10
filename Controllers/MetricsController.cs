// using Greenhouse.Data;
// using Greenhouse.Data.Interfaces;
// using Microsoft.AspNetCore.Mvc;
//
// namespace Greenhouse.Controllers;
//
// [ApiController]
// [Route("api/[controller]")]
// public class MetricsController(IGreenhouseMetricService igreenhouseMetricService) : Controller
// {
//     [HttpGet]
//     public async Task<List<GreenhouseMetric>> Get()
//     {
//         return await Task.FromResult(igreenhouseMetricService.GetGreenhouseMetrics());
//     }
// }
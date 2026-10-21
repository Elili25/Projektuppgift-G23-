
using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models;
using SeventWebApplication.Models.VehicleNS;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace SeventWebApplication.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // TESTDATA: Lägg till dummybilen.
            FleetManager.instance.SeedTestVehicle();

            return View();
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

        [HttpGet]
        //[HttpPost]
        public IActionResult Search(string query)
        {
            // GAMMAL KOD – kommenterad eftersom FindVehiclesFromRequest()
            // inte finns i FleetManager.

            /*
            var result = FleetManager.instance.FindVehiclesFromRequest(Request);

            var searchResults = result
                .Select(vehicle => new VehicleSearchResult(vehicle))
                .ToList();

            return Json(searchResults);
            */

            // NY KOD – använder FindVehicles(string query) i FleetManager.
            var result = FleetManager.instance.FindVehicles(query);

            var searchResults = result
                .Select(vehicle => new VehicleSearchResult(vehicle))
                .ToList();

            return Json(searchResults);
        }

    }


    public record VehicleSearchResult
    {
        public VehicleSearchResult(string brand, string model)
        {
            Brand = brand;
            Model = model;
        }

        public VehicleSearchResult(Vehicle vehicle)
        {
            Brand = vehicle.Model.Brand.BrandName;
            Model = vehicle.Model.ModelName;
        }

        // ÄNDRING: Egenskaper i stället för publika fält,
        // så att JSON-serialiseringen inkluderar värdena.
        public string Brand { get; init; }
        public string Model { get; init; }
    }
}

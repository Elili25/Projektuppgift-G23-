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
        public IActionResult Search( string query )
        {

            var result = FleetManager.instance.FindVehiclesFromRequest( Request );


            Console.WriteLine( result );
            Console.WriteLine( ((List<VehicleSearchResult>)Json( result ).Value)[0] );

            return new JsonResult(result.ToList());
        }

    }
    public record VehicleSearchResult
    {
        public VehicleSearchResult(string brand, string model )
        {
            this.brand = brand;
            this.model = model;
        }
        public VehicleSearchResult(Vehicle vehicle)
        {
            brand = vehicle.Brand;
            model = vehicle.Model;
        }
        public string brand;
        public string model;
    }
}

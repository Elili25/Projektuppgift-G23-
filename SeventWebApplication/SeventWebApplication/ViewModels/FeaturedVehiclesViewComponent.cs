using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.ViewComponents
{
    // Startsidans "Featured vehicles". Hämtar seedade familjer och bilar via VehicleFamilyService
    // och renderar Views/Shared/Components/FeaturedVehicles/Default.cshtml
    public class FeaturedVehiclesViewComponent : ViewComponent
    {
        private readonly VehicleFamilyService _familyService;

        public FeaturedVehiclesViewComponent(VehicleFamilyService familyService)
        {
            _familyService = familyService;
        }

        public IViewComponentResult Invoke()
        {
            return View(_familyService.GetFeaturedVehicles());
        }
    }
}
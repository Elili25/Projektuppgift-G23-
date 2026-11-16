using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.ViewComponents
{
    // Startsidans "Vehicle categories": sex rutor med data från seedade familjer och fordon.
    // Renderar Views/Shared/Components/VehicleCategories/Default.cshtml
    public class VehicleCategoriesViewComponent : ViewComponent
    {
        private readonly VehicleFamilyService _familyService;

        public VehicleCategoriesViewComponent(VehicleFamilyService familyService)
        {
            _familyService = familyService;
        }

        public IViewComponentResult Invoke()
        {
            return View(_familyService.GetVehicleCategoryTiles());
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.ViewComponents
{
    public class VehicleCategoriesViewComponent : ViewComponent
    {
        private readonly VehicleFamilyService _familyService;

        public VehicleCategoriesViewComponent(VehicleFamilyService familyService)
        {
            _familyService = familyService;
        }

        public IViewComponentResult Invoke()
        {
            return View(_familyService.GetVehicleCategories());
        }
    }
}
using System;


namespace SeventWebApplication.VehiclePackageFactory
{
    public class ElectricPackageFactory : IVehiclePackageFactory
    {
        public ElectricPackageFactory() { }

        void /*Vehicle*/ IVehiclePackageFactory.CreateVehicle( String brand )
        {

        }

        void /*IPricingStrategy*/ IVehiclePackageFactory.CreatePricingStrategy()
        {

        }

        void /*IEnumerable<IAddon>*/ IVehiclePackageFactory.CreateDefaultAddons()
        {

        }
    }
}

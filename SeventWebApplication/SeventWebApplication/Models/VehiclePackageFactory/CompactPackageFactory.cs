using System;


namespace SeventWebApplication.VehiclePackageFactory
{
    public class CompactPackageFactory : IVehiclePackageFactory
    {
        public CompactPackageFactory() { }

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

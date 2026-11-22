using System;


namespace SeventWebApplication.VehiclePackageFactory
{
	public class VanPackageFactory : IVehiclePackageFactory
	{
		public VanPackageFactory() { }

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
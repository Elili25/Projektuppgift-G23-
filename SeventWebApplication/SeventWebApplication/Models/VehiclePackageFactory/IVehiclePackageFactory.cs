using System;

namespace SeventWebApplication.VehiclePackageFactory
{
	public interface IVehiclePackageFactory
	{
        // Borde inte skapa en bil, borde hitta en bil
        void /*Vehicle*/ CreateVehicle( String brand ); 

		void /*IPricingStrategy*/ CreatePricingStrategy();

		void /*IEnumerable<IAddon>*/ CreateDefaultAddons();
	}
}

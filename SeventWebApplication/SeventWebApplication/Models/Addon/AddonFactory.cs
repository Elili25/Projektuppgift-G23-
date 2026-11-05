using SeventWebApplication.Models.Addon.Interfaces;
using System.Reflection;  // Gör att koden kan inspektera & analysera sig själv

namespace SeventWebApplication.Models.Addon
{
    public static class AddonFactory
    {
        // En cache som håller reda på alla tillgängliga addon-typer
        private static readonly Dictionary<string, Type> _addonTypes;

        static AddonFactory()
        {
            // Hämtar alla klasser som implementerar IAddon och som inte är abstrakta eller gränssnitt
            _addonTypes = Assembly.GetExecutingAssembly()
             .GetTypes()
             .Where(t => typeof(IAddon).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
             .ToDictionary(
                 // Nyckeln blir klassnamnet i små bokstäver utan "Addon" (t.ex. "childseat")
                 t => t.Name.Replace("Addon", "").Trim().ToLower(),
                 t => t
             );
        }

        public static IAddon Create(string addonKey, int quantity)
        {
            // Det antas att nyckeln är giltig eftersom ett interface används för input

            // Skapar en ny instans av addonklassen dynamiskt via Reflection
            var addon = (IAddon)Activator.CreateInstance(_addonTypes[addonKey.Trim().ToLower()])!;
            addon.Quantity = quantity;

            return addon;
        }
    }
}

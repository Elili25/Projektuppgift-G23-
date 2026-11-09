using SeventWebApplication.Models.Addon.Interfaces;
using System.Reflection;

namespace SeventWebApplication.Models.Addon
{
    public static class AddonFactory
    {
        private static readonly Dictionary<string, Type> _addonTypes;

        static AddonFactory()
        {
            _addonTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IAddon).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToDictionary(
                    t => t.Name.Replace("Addon", "").Trim().ToLowerInvariant(),
                    t => t);
        }

        public static IAddon Create(string addonKey, int quantity)
        {
            if (string.IsNullOrWhiteSpace(addonKey)
                || !_addonTypes.TryGetValue(addonKey.Trim().ToLowerInvariant(), out var addonType))
            {
                throw new ArgumentException($"Unknown addon: {addonKey}", nameof(addonKey));
            }

            var addon = (IAddon)Activator.CreateInstance(addonType)!;
            addon.Quantity = quantity;
            return addon;
        }
    }
}

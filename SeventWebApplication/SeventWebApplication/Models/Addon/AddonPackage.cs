using SeventWebApplication.Models.Addon.Interfaces;

namespace SeventWebApplication.Models.Addon
{
    public class AddonPackage
    {
        // Samling med alla addons som ska läggas till i bokningen
        private readonly List<IAddon> _addons = new();

        // En public men skrivskyddad lista som andra klasser kan läsa av
        public IReadOnlyCollection<IAddon> Items => _addons.AsReadOnly();

        public void Add(IAddon addon)
        {
            // Letar efter en existerande addon av samma typ
            var existingAddon = _addons.FirstOrDefault(a => a.AddonType == addon.AddonType);

            if (existingAddon != null)
            {
                // Om den redan finns, plussa på antalet från den nya
                existingAddon.Quantity += addon.Quantity;
            }
            else
            {
                // Om den inte finns, lägg till hela objektet i listan
                _addons.Add(addon);
            }
        }

        public void Remove(IAddon addon)
        {
            var existingAddon = _addons.FirstOrDefault(a => a.AddonType == addon.AddonType);

            if (existingAddon != null)
            {
                // Minskar antalet
                existingAddon.Quantity -= addon.Quantity;

                // Om antalet går ner till 0 eller mindre, tar bort objektet från listan
                if (existingAddon.Quantity <= 0)
                {
                    _addons.Remove(existingAddon);
                }
            }
        }

        // Slår samman priset för alla addons
        public decimal GetTotalAddonPrice()
        {
            decimal total = 0;
            foreach (var addon in _addons)
            {
                total += addon.GetPrice();
            }
            return total;
        }
    }

}

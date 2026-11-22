namespace SeventWebApplication.Models.Users
{
    // Det personal behöver kunna göra med kunder när de skapar en bokning:
    // söka bland befintliga kunder, och skapa en kund (eller en dummy) som inte har något konto.
    // Metoderna tar IBookingManager, så bara personal kan anropa dem (samma upplägg som BookingManagementService).
    public class StaffCustomerService
    {
        public const int MaxSearchResults = 10;

        private readonly IUserRepository _users;

        public StaffCustomerService(IUserRepository users)
        {
            _users = users;
        }

        // Aktiva kunder som matchar namn, e-post eller telefon. Dummy-konton tas inte med.
        public IReadOnlyList<User> SearchCustomers(IBookingManager actor, string? query)
        {
            EnsureStaff(actor);

            string q = query?.Trim() ?? "";
            if (q.Length < 2)
                return Array.Empty<User>();

            return _users.GetAll()
                .Where(u => u.Role == Role.Customer && u.IsActive && !u.IsDummy)
                .Where(u => u.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                         || u.Mail.Contains(q, StringComparison.OrdinalIgnoreCase)
                         || u.Phone.Contains(q, StringComparison.OrdinalIgnoreCase))
                .OrderBy(u => u.Name)
                .Take(MaxSearchResults)
                .ToList();
        }

        // Kastar ArgumentException/InvalidOperationException om uppgifterna inte duger. Skapar inget.
        // Kund: namn och giltig e-post krävs, och e-posten får inte redan finnas.
        // Dummy: bara namn krävs. Saknas e-post får kontot en påhittad adress som inte går att nå.
        public void Validate(string? name, string? mail, string? phone, bool isDummy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Enter the name.");

            ContactValidator.EnsurePhone(phone);

            if (!isDummy || !string.IsNullOrWhiteSpace(mail))
            {
                if (string.IsNullOrWhiteSpace(mail))
                    throw new ArgumentException("Enter the email address.");

                ContactValidator.EnsureMail(mail);

                if (_users.GetByMail(mail) is not null)
                    throw new InvalidOperationException(
                        "An account with that email address already exists. Search for the customer above instead.");
            }
        }

        // Skapar en kund som inte kan logga in. isDummy = testbokning utan riktig kund.
        public User CreateWithoutLogin(IBookingManager actor, string? name, string? mail, string? phone, bool isDummy)
        {
            EnsureStaff(actor);
            Validate(name, mail, phone, isDummy);

            string address = string.IsNullOrWhiteSpace(mail)
                ? $"dummy-{Guid.NewGuid():N}@dummy.invalid"
                : mail;

            // Slumpat lösenord som ingen känner till. CanLogin = false stoppar inloggning ändå.
            var customer = UserFactory.Create(
                Role.Customer, Guid.NewGuid(), name!, phone ?? "", address, Guid.NewGuid().ToString("N"));
            customer.MarkNoLogin(isDummy);

            _users.Add(customer);
            return customer;
        }

        private void EnsureStaff(IBookingManager actor)
        {
            ArgumentNullException.ThrowIfNull(actor);

            User? current = _users.GetById(actor.UserId);
            if (current is not IBookingManager || !current.IsActive)
                throw new UnauthorizedAccessException("Only active staff can do this.");
        }
    }
}

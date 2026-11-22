namespace SeventWebApplication.Models.Users
{
    // Sköter inloggning och registrering. User-klassen ska inte behöva bry sig om
    // hur inloggning funkar, så det ligger här istället (High Cohesion).
    public class AuthService
    {
        private readonly IUserRepository _users;

        public AuthService(IUserRepository users)
        {
            _users = users;
        }

        // Ger tillbaka användaren om allt stämmer, annars kastas ett fel.
        // Samma felmeddelande oavsett om det är mailen eller lösenordet som är fel,
        // så man inte kan lista ut vilka mailadresser som har konton.
        public User Login(string mail, string password)
        {
            User? user = _users.GetByMail(mail);

            if (user is null || !user.CanLogin || !user.CheckPassword(password))
                throw new InvalidOperationException("Wrong email address or password.");

            if (!user.IsActive)
                throw new InvalidOperationException("The account is deactivated. Please contact our staff.");

            return user;
        }

        // Registrerar man sig själv blir man alltid kund.
        // Anställda får skapas av en admin och som vi har det nu finns endast ett seedat admin konto.
        public Customer Register(string name, string mail, string phone, string password)
        {
            ContactValidator.EnsureMail(mail);
            ContactValidator.EnsurePhone(phone);

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new ArgumentException("The password must be at least 8 characters.", nameof(password));

            if (_users.GetByMail(mail) is not null)
                throw new InvalidOperationException("An account with that email address already exists.");

            var customer = (Customer)UserFactory.Create(
                Role.Customer, Guid.NewGuid(), name, phone, mail, password);

            _users.Add(customer);
            return customer;
        }
    }
}

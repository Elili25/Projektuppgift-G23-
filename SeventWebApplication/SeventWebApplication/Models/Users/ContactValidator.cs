using System.Text.RegularExpressions;

namespace SeventWebApplication.Models.Users
{
    // Regler för e-post och telefonnummer på ett ställe, så registrering, admin-skapande och redigering
    // kontrollerar samma sak. Reglerna körs även i User, så ingen väg runt dem finns.
    public static class ContactValidator
    {
        public const string InvalidMailMessage = "Enter a valid email address, e.g. name@example.com.";
        public const string InvalidPhoneMessage = "Enter a valid phone number, e.g. 070-123 45 67 or +46 70 123 45 67.";

        // namn@domän.tld, utan mellanslag och med exakt ett @
        private static readonly Regex MailPattern = new(
            @"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Valfritt +, sedan siffror med mellanslag, bindestreck eller parenteser emellan. 7-15 siffror totalt.
        private static readonly Regex PhonePattern = new(
            @"^\+?[0-9][0-9 \-()]*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool IsValidMail(string? mail) =>
            !string.IsNullOrWhiteSpace(mail) && mail.Trim().Length <= 254 && MailPattern.IsMatch(mail.Trim());

        // Telefon är valfritt (tomt är ok), men om något står där måste det vara ett riktigt nummer
        public static bool IsValidPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            string trimmed = phone.Trim();
            if (!PhonePattern.IsMatch(trimmed))
                return false;

            int digits = trimmed.Count(char.IsDigit);
            return digits is >= 7 and <= 15;
        }

        public static void EnsureMail(string? mail)
        {
            if (!IsValidMail(mail))
                throw new ArgumentException(InvalidMailMessage);
        }

        public static void EnsurePhone(string? phone)
        {
            if (!IsValidPhone(phone))
                throw new ArgumentException(InvalidPhoneMessage);
        }
    }
}

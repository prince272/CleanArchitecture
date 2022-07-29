using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Helpers
{
    public static class ContactHelper
    {
        public static ContactType Switch(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                throw new ArgumentException($"Value cannot be null or whitespace.", nameof(rawValue));

            return Regex.IsMatch(rawValue.ToLowerInvariant(), "^[-+0-9() ]+$") ? ContactType.PhoneNumber : ContactType.EmailAddress;
        }

        public static PhoneNumbers.PhoneNumber ValidatePhoneNumber(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                throw new ArgumentException($"Value cannot be null or whitespace.", nameof(rawValue));

            var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
            var phoneNumberValue = phoneNumberUtil.ParseAndKeepRawInput(rawValue, null);

            if (phoneNumberUtil.IsValidNumber(phoneNumberValue))
                return phoneNumberValue;

            throw new FormatException($"Input '{rawValue}' was not recognized as a valid PhoneNumber.");
        }

        public static bool TryValidatePhoneNumber(string rawValue, out PhoneNumbers.PhoneNumber phoneNumber)
        {
            try
            {
                phoneNumber = ValidatePhoneNumber(rawValue);
                return true;
            }
            catch (Exception)
            {
                phoneNumber = null!;
                return false;
            }
        }

        // C# code to validate email address
        // source: https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address
        public static System.Net.Mail.MailAddress ValidateEmailAddress(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                throw new ArgumentException($"Value cannot be null or whitespace.", nameof(rawValue));

            if (!rawValue.EndsWith("."))
            {
                var emailAddress = new System.Net.Mail.MailAddress(rawValue);
                if (emailAddress.Address == rawValue)
                {
                    return emailAddress;
                }
            }

            throw new FormatException($"Input '{rawValue}' was not recognized as a valid MailAddress.");
        }

        public static bool TryValidateEmailAddress(string rawValue, out System.Net.Mail.MailAddress emailAddress)
        {
            try
            {
                emailAddress = ValidateEmailAddress(rawValue);
                return true;
            }
            catch (Exception)
            {
                emailAddress = null!;
                return false;
            }
        }
    }

    public enum ContactType
    {
        PhoneNumber,
        EmailAddress
    }
}

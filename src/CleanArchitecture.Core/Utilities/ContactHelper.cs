using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Utilities
{
    public static class ContactHelper
    {
        public static ContactType GetContactType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Value cannot be null or whitespace.", nameof(value));

            return Regex.IsMatch(value.ToLowerInvariant(), "^[-+0-9() ]+$") ? ContactType.PhoneNumber : ContactType.EmailAddress;
        }

        public static (string Number, PhoneNumber Details) ParsePhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Value cannot be null or whitespace.", nameof(value));

            var phoneNumberHelper = PhoneNumberUtil.GetInstance();
            var phoneNumberDetails = phoneNumberHelper.ParseAndKeepRawInput(value, null);

            if (phoneNumberHelper.IsValidNumber(phoneNumberDetails) && phoneNumberDetails.RawInput == value)
            {
                return ($"+{phoneNumberDetails.CountryCode}{phoneNumberDetails.NationalNumber}", phoneNumberDetails);
            }

            throw new FormatException($"Input '{value}' was not recognized as a valid PhoneNumber.");
        }

        public static bool TryParsePhoneNumber(string value, out (string Number, PhoneNumber Details) phoneDetails)
        {
            try
            {
                phoneDetails = ParsePhoneNumber(value);
                return true;
            }
            catch (Exception)
            {
                phoneDetails = (null!, null!);
                return false;
            }
        }

        // C# code to validate email address
        // source: https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address
        public static (string Address, System.Net.Mail.MailAddress Details) ParseEmailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Value cannot be null or whitespace.", nameof(value));

            if (!value.EndsWith("."))
            {
                var emailAddressDetails = new System.Net.Mail.MailAddress(value);
                if (emailAddressDetails.Address == value)
                {
                    return ($"{emailAddressDetails.User}@{emailAddressDetails.Host}".ToLowerInvariant(), emailAddressDetails);
                }
            }

            throw new FormatException($"Input '{value}' was not recognized as a valid MailAddress.");
        }

        public static bool TryParseEmailAddress(string value, out (string Address, System.Net.Mail.MailAddress Details) emailDetails)
        {
            try
            {
                emailDetails = ParseEmailAddress(value);
                return true;
            }
            catch (Exception)
            {
                emailDetails = (null!, null!);
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

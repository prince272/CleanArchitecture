using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider
{
    
    public class PaymentResult
    {
        protected PaymentResult()
        {

        }

        [MemberNotNullWhen(false, nameof(Errors))]
        public bool Success { get; protected set; }

        public string? Message { get; protected set; }

        public IDictionary<string, string[]>? Errors { get; protected set; }

        public IDictionary<string, object>? Data { get; protected set; }

        public static PaymentResult Failed(IDictionary<string, string[]>? errors = null, string? message = null)
        {
            return new PaymentResult()
            {
                Success = false,
                Errors = errors ?? new Dictionary<string, string[]>(),
                Message = message
            };
        }

        public static PaymentResult Succeeded(IDictionary<string, object>? data = null, string? message = null)
        {
            return new PaymentResult()
            {
                Success = true,
                Data = data ?? new Dictionary<string, object>(),
                Message = message
            };
        }

        /// <summary>
        /// Converts the value of the current <see cref="PaymentResult"/> object to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the current <see cref="PaymentResult"/> object.</returns>
        /// <remarks>
        /// If the operation was successful the ToString() will return "Succeeded" otherwise it returned
        /// "Failed : " followed by a comma delimited list of error codes from its <see cref="Errors"/> collection, if any.
        /// </remarks>
        public override string ToString()
        {
            return Success ?
                   "Succeeded" :
                   string.Format(CultureInfo.InvariantCulture, "{0} : {1}", "Failed", string.Join(",", Errors!.Keys.ToList()));
        }
    }
}

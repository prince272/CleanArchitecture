using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Entities
{
    [Owned]
    public class PaymentIssuer
    {
        public string Code { get; set; } = null!;

        public string Pattern { get; set; } = null!;

        public string Name { get; set; } = null!;
    }
}

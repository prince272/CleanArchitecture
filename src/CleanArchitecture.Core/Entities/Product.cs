using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Entities
{
    public class Product : IEntity
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public decimal Price { get; set; }
    }
}

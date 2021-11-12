using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarShowcase.Domain
{
    public class Settings
    {
        public StellarSettings Stellar { get; set; }
    }

    public class StellarSettings
    {
        public string NodeUrl { get; set; }
    }
}

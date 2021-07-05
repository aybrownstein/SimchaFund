using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimchaFund.web.Models
{
    public class ContributionsViewModel
    {
        public Simcha Simcha { get; set; }
        public List<SimchaContributor> Contributors { get; set; }
    }
}

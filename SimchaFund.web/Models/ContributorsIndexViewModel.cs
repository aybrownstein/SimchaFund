using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimchaFund.web.Models
{
    public class ContributorsIndexViewModel
    {
        public List<Contributors> Contributors { get; set; }
        public decimal Total { get; set; }
    }
}

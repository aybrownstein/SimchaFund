using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimchaFund.web.Models
{
    public class HistoryModel
    {
        public List<Transaction> Transactions { get; set; }
        public decimal Balance { get; set; }
        public string ContributorName { get; set; }
    }
}

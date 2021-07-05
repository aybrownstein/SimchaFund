using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SimchaFund.web.Models
{
    public class SimchaModel
    {
        public List<Simcha> Simchas { get; set; }
        public int TotalContributors { get; set; }
    }
    public class Contributors
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PhoneNumber { get; set; }
        public bool AlwaysInclude { get; set; }
        public decimal Balance { get; set; }

    }

    public class SimchaContributor
    {
        public int ContributorId { get; set; }
        public string Name { get; set; }
        public bool AlwaysInclude { get; set; }
        public decimal? Amount { get; set; }
        public decimal Balance { get; set; }
    }

    public class Contributions
    {
        public decimal ContributionAmount { get; set; }
        public DateTime ContributionDate { get; set; }
        public int SimchaId { get; set; }
        public int ContributorId { get; set; }
        public string SimchaName { get; set; }
    }

    public class Simcha
    {
        public int Id { get; set; }
        public string SimchaName { get; set; }
        public DateTime SimchaDate { get; set; }

        public int ContributorAmount { get; set; }
        public decimal Total { get; set; }
    }


    public class Deposit
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public int ContributorId { get; set; }
        public DateTime DepositDate { get; set; }
    }

    public class ContributionInclusion
    {
        public bool Include { get; set; }
        public decimal Amount { get; set; }
        public int ContributorId { get; set; }
    }

    public class Transaction
    {
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
  
}

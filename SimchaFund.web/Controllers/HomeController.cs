using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimchaFund.web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SimchaFund.web.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString = @"Data Source=.\sqlexpress;Initial Catalog=SimchaFund; Integrated Security=true;";

        public IActionResult Index()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            var db = new SimchaDb(_connectionString);
            var model = new SimchaModel { Simchas = db.GetSimchas(), TotalContributors = db.GetContributorCount() };
            return View(model);
        }
        [HttpPost]
        public IActionResult New(Simcha simcha)
        {
            SimchaDb db = new(_connectionString);
            db.AddSimcha(simcha);
            TempData["Message"] = $"New Simcha Created!";
            return Redirect("/");
        }

       public IActionResult Contributions(int simchaId)
        {
            SimchaDb db = new SimchaDb(_connectionString);
            var simcha = db.GetSimchaById(simchaId);
            var contributors = db.GetSimchaContributors(simchaId);

            var vm = new ContributionsViewModel
            {
                Contributors = contributors,
                Simcha = simcha
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult UpdateContributions(List<ContributionInclusion> contributors, int simchaId)
        {
            SimchaDb db = new SimchaDb(_connectionString);
            db.UpdateSimchaContributions(simchaId, contributors);
            TempData["Message"] = "Simcha Updated Successfully";
            return Redirect("/");
        }

        public IActionResult Contributors()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            SimchaDb db = new SimchaDb(_connectionString);
            var vm = new ContributorsIndexViewModel
            {
                Contributors = db.GetContributors(),
                Total = db.GetTotal()
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult NewContributor(Contributors contributor, decimal initialDeposit)
        {
            var db = new SimchaDb(_connectionString);
            db.AddContributor(contributor);
            Deposit deposit = new Deposit
            {
                Amount = initialDeposit,
                ContributorId = contributor.Id,
            };
            db.Transaction(deposit);
            TempData["Message"] = "NEW Contributor Created!";
            return Redirect("/Home/Contributors");
        }

        [HttpPost]
        public IActionResult Edit(Contributors contributor)
        {
            SimchaDb db = new SimchaDb(_connectionString);
            db.UpdateContributor(contributor);
            TempData["Message"] = "Contributor Updated Successfully";
            return Redirect("/Home/Contributors");
        }

        public IActionResult History(int contribId)
        {
            var db = new SimchaDb(_connectionString);
            List<Deposit> deposits = db.GetDepositById(contribId);
            List<Contributions> contributions = db.GetContributionsById(contribId);

            var transactions = deposits.Select(d => new Transaction
            {
                Action = "Deposit",
                Amount = d.Amount,
                Date = d.DepositDate
            }).Concat(contributions.Select(c => new Transaction
            {
                Action = $"Contribution for the {c.SimchaName} simcha",
                Amount = -c.ContributionAmount,
                Date = c.ContributionDate
            })).OrderByDescending(t => t.Date).ToList();

            var model = new HistoryModel
            {
                Transactions = transactions,
                Balance = transactions.Sum(t => t.Amount),
                ContributorName = db.GetContributorName(contribId)
        };
            return View(model);
        }

        public IActionResult Deposit(Deposit deposit)
        {
            var db = new SimchaDb(_connectionString);
            db.Transaction(deposit);
            TempData["Message"] = "Deposit Successfully recorded";
            return Redirect("/Home/Contributors");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SimchaFund.web.Models
{
    public class SimchaDb
    {

        private readonly string _connectionString;

        public SimchaDb(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int GetContributorCount()
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Contributors";
            connection.Open();
            return (int)cmd.ExecuteScalar();
        }

        public List<Simcha> GetSimchas()
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT *, (
                              SELECT ISNULL(SUM(Contribution), 0)
                              FROM Contributions WHERE SimchaId = s.Id) as 'Total',
                             (SELECT COUNT(*) FROM Contributions WHERE SimchaId = s.Id)
                             AS 'ContributionAmount' FROM Simchas s";
            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<Simcha> simchas = new List<Simcha>();
            while (reader.Read())
            {

                simchas.Add(new Simcha
                {
                    Id = (int)reader["Id"],
                    SimchaName = (string)reader["SimchaName"],
                    SimchaDate = (DateTime)reader["SimchaDate"],
                    ContributorAmount = (int)reader["ContributionAmount"],
                    Total = (decimal)reader["Total"]
                });
            }
            return simchas;
        }

        private int GetContributorAmount(int simchaId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Contributions Where SimchaId = @simchaId";
            cmd.Parameters.AddWithValue("@simchaId", simchaId);
            connection.Open();
            return (int)cmd.ExecuteScalar();
        }

        private decimal GetTotal(int simchaId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT SUM(*) FROM Contributions Where SimchaId = @simchaId";
            cmd.Parameters.AddWithValue("@simchaId", simchaId);
            connection.Open();
            return (int)cmd.ExecuteScalar();
        }

        public void AddSimcha(Simcha simcha)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Simchas (SimchaName, SimchaDate) " +
                " Values (@simchaName, @simchaDate)";
            cmd.Parameters.AddWithValue("@simchaName", simcha.SimchaName);
            cmd.Parameters.AddWithValue("@simchaDate", simcha.SimchaDate);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void AddContributor(Contributors contributor)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Contributors (Name, PhoneNumber, AlwaysInclude) " +
                             " Values (@name, @phoneNumber @alwaysInclude)";
            cmd.Parameters.AddWithValue("@name", contributor.Name);
            cmd.Parameters.AddWithValue("@phoneNumber", contributor.PhoneNumber);
            cmd.Parameters.AddWithValue("@alwaysInclude", contributor.AlwaysInclude);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public List<Contributors> GetContributors()
        {
            var contributors = new List<Contributors>();
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"SELECT *,(
(SELECT ISNULL(SUM(d.Deposit), 0) FROM Deposits d WHERE d.ContributorId = c.Id) - 
(SELECT ISNULL(SUM(co.Contribution), 0) FROM Contributions co WHERE co.ContributorId = c.Id))
AS 'Balance' FROM Contributors c";
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var contributor = new Contributors();
                contributor.Id = (int)reader["Id"];
                contributor.Name = (string)reader["Name"];
                contributor.PhoneNumber = (int)reader["PhoneNumber"];
                contributor.AlwaysInclude = (bool)reader["AlwaysInclude"];
                contributor.Balance = (decimal)reader["Balance"];
                contributors.Add(contributor);
            }
            return contributors;
        }

        public List<SimchaContributor> GetSimchaContributors(int simchaId)
        {
            List<Contributors> contributors = GetContributors();
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Contributions WHERE SimchaId = @simchaId";
            cmd.Parameters.AddWithValue("@simchaId", simchaId);
            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<Contributions> contributions = new List<Contributions>();
            while (reader.Read())
            {
                Contributions contribution = new Contributions
                {
                    ContributionAmount = (decimal)reader["Contribution"],
                    SimchaId = (int)reader["SimchaId"],
                    ContributorId = (int)reader["ContributorId"]
                };
                contributions.Add(contribution);
            }

            return contributors.Select(contributor =>
            {
                var sc = new SimchaContributor();
                sc.Name = contributor.Name;
                sc.AlwaysInclude = contributor.AlwaysInclude;
                sc.ContributorId = contributor.Id;
                sc.Balance = contributor.Balance;
                Contributions contribution = contributions.FirstOrDefault(c => c.ContributorId == contributor.Id);
                if (contribution != null)
                {
                    sc.Amount = contribution.ContributionAmount;
                }
                return sc;
            }).ToList();
        }

        public Simcha GetSimchaById(int simchaId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT *, 
 (SELECT ISNULL(SUM(Contribution), 0) FROM Contributions WHERE SimchaId = s.Id) AS 'Total',
(SELECT COUNT(*) FROM Contributions WHERE SImchaId = s.Id) AS 'ContributorAmount'
FROM Simchas s WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", simchaId);
            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }
            Simcha simcha = new Simcha();
            simcha.Id = (int)reader["Id"];
            simcha.SimchaDate = (DateTime)reader["SimchaDate"];
            simcha.SimchaName = (string)reader["SimchaName"];
            simcha.ContributorAmount = (int)reader["ContributorAmount"];
            simcha.Total = (decimal)reader["Total"];
            return simcha;
        }

        public void Transaction(Deposit deposit)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Deposits (Deposit, ContributorId,DepositDate) " +
                            " VALUES (@deposit, @contributorId, GETDATE())";
            cmd.Parameters.AddWithValue("@deposit", deposit.Amount);
            cmd.Parameters.AddWithValue("@contributorId", deposit.ContributorId);
            cmd.ExecuteNonQuery();
        }

        public Contributors byId(int id)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT c.Contribution, c.ContributionDate, s.SimchaName, d.Deposit, d.DepositDate, cr.Name FROM Contributions c " +
                " JOIN Contributors cr ON cr.Id = c.ContributorId JOIN Simchas s ON s.Id = c.SimchaId " +
                " JOIN Deposits d ON cr.Id = d.ContributorId  WHERE @id = cr.Id";
            cmd.Parameters.AddWithValue("@id", id);
            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            Contributors contributor = new Contributors
            {
                Id = id,
                Name = (string)reader["Name"],
                PhoneNumber = (int)reader["PhoneNumber"],
                AlwaysInclude = (bool)reader["AlwaysInclude"]
            };
            return contributor;
        }
        public void UpdateSimchaContributions(int simchaId, List<ContributionInclusion> contributors)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Contributions WHERE SimchaId = @simchaId";
            cmd.Parameters.AddWithValue("@simchaId", simchaId);
            connection.Open();
            cmd.ExecuteNonQuery();

            cmd.Parameters.Clear();
            cmd.CommandText = @"Insert INTO Contrinutions (SimchaId, ContributorId, Contribution, ContributionDate)
                            Values (@simchaId, @ContributorId, @contribution, GETDATE())";
            foreach (var contributor in contributors.Where(c => c.Include))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@simchaId", simchaId);
                cmd.Parameters.AddWithValue("@contributorId", contributor.ContributorId);
                cmd.Parameters.AddWithValue("@Contribution", contributor.Amount);
                cmd.ExecuteNonQuery();
            }
        }
        public decimal GetTotal()
        {
            return GetTotalDeposits() - GetTotalContributions();
        }

        private decimal GetTotalDeposits()
        {

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT ISNULL(SUM(Deposit), 0) FROM Deposits";
            connection.Open();
            return (decimal)cmd.ExecuteScalar();
        }

        private decimal GetTotalContributions()
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT ISNULL(SUM(Contribution), 0) FROM Contributions";
            connection.Open();
            return (decimal)cmd.ExecuteScalar();
        }

        public void UpdateContributor(Contributors contributor)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"UPDATE CONTRIBUTORS SET Name = @name, PhoneNumber = @phoneNumber,
                          AlwaysInclude = @alwaysInclude, WHERE Id = @id";
            cmd.Parameters.AddWithValue("@name", contributor.Name);
            cmd.Parameters.AddWithValue("@phoneNumber", contributor.PhoneNumber);
            cmd.Parameters.AddWithValue("@alwaysInclude", contributor.AlwaysInclude);
            cmd.Parameters.AddWithValue("@id", contributor.Id);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public List<Deposit> GetDepositById(int contribId)
        {
            List<Deposit> deposits = new();
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * from DEPOSIT WHERE ContributorId = @contribId";
            cmd.Parameters.AddWithValue("@contribId", contribId);
            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Deposit deposit = new Deposit();
                deposit.Id = (int)reader["Id"];
                deposit.Amount = (decimal)reader["Deposit"];
                deposit.DepositDate = (DateTime)reader["DepositDate"];
                deposits.Add(deposit);
            }
            return deposits;
        }

        public List<Contributions> GetContributionsById(int contribId)
        {
            List<Contributions> contributions = new List<Contributions>();
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT c.*, s.SimchaName, s.SimchaDate, FROM Contributions c
                           JOIN Simchas s On c.SimchaId = s.Id
                           WHERE c. ContributorId = contribId";
            cmd.Parameters.AddWithValue("@contribId", contribId);
            connection.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var contribution = new Contributions();
                contribution.ContributorId = (int)reader["ContributorId"];
                contribution.ContributionAmount = (decimal)reader["Contribution"];
                contribution.ContributionDate = (DateTime)reader["ContributionDate"];
                contribution.SimchaId = (int)reader["SimchaId"];
                contribution.SimchaName = (string)reader["SimchaName"];
                contributions.Add(contribution);
            }
            return contributions;
        }
        public string GetContributorName(int contribId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Name FROM Contributors WHERE Id = @id";
            cmd.Parameters.AddWithValue("@Id", contribId);
            connection.Open();
            return (string)cmd.ExecuteScalar();
        }
    }
    }


using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using OfficeManagerWPF.Models;

namespace OfficeManagerWPF.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private const string DatabaseFileName = "OfficeManager.db";

        public DatabaseService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "OfficeManager", DatabaseFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            _connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Companies 테이블
                string createCompaniesTable = @"
                    CREATE TABLE IF NOT EXISTS Companies (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Type TEXT NOT NULL,
                        ContractDate TEXT NOT NULL,
                        MonthlyFee REAL NOT NULL,
                        ContactPerson TEXT,
                        PhoneNumber TEXT,
                        Email TEXT,
                        Notes TEXT,
                        IsActive INTEGER NOT NULL DEFAULT 1
                    )";

                // Payments 테이블
                string createPaymentsTable = @"
                    CREATE TABLE IF NOT EXISTS Payments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CompanyId INTEGER NOT NULL,
                        CompanyName TEXT NOT NULL,
                        PaymentDate TEXT NOT NULL,
                        Amount REAL NOT NULL,
                        Period TEXT NOT NULL,
                        PaymentMethod TEXT,
                        Notes TEXT,
                        IsConfirmed INTEGER NOT NULL DEFAULT 1,
                        FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
                    )";

                // Expenses 테이블
                string createExpensesTable = @"
                    CREATE TABLE IF NOT EXISTS Expenses (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ExpenseDate TEXT NOT NULL,
                        Category TEXT NOT NULL,
                        Amount REAL NOT NULL,
                        Description TEXT NOT NULL,
                        Period TEXT NOT NULL,
                        Notes TEXT
                    )";

                ExecuteNonQuery(createCompaniesTable, connection);
                ExecuteNonQuery(createPaymentsTable, connection);
                ExecuteNonQuery(createExpensesTable, connection);
            }
        }

        // Company CRUD
        public List<Company> GetAllCompanies()
        {
            var companies = new List<Company>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Companies";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        companies.Add(new Company
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Type = reader.GetString(2),
                            ContractDate = DateTime.Parse(reader.GetString(3)),
                            MonthlyFee = reader.GetDecimal(4),
                            ContactPerson = reader.IsDBNull(5) ? null : reader.GetString(5),
                            PhoneNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Email = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                            IsActive = reader.GetInt32(9) == 1
                        });
                    }
                }
            }
            return companies;
        }

        public void AddCompany(Company company)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Companies (Name, Type, ContractDate, MonthlyFee, ContactPerson, 
                                PhoneNumber, Email, Notes, IsActive) 
                                VALUES (@Name, @Type, @ContractDate, @MonthlyFee, @ContactPerson, 
                                @PhoneNumber, @Email, @Notes, @IsActive)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    AddCompanyParameters(command, company);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateCompany(Company company)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"UPDATE Companies SET Name=@Name, Type=@Type, ContractDate=@ContractDate, 
                                MonthlyFee=@MonthlyFee, ContactPerson=@ContactPerson, PhoneNumber=@PhoneNumber, 
                                Email=@Email, Notes=@Notes, IsActive=@IsActive WHERE Id=@Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    AddCompanyParameters(command, company);
                    command.Parameters.AddWithValue("@Id", company.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteCompany(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Companies WHERE Id=@Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Payment CRUD
        public List<Payment> GetPaymentsByPeriod(string period)
        {
            var payments = new List<Payment>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Payments WHERE Period=@Period";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Period", period);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payments.Add(new Payment
                            {
                                Id = reader.GetInt32(0),
                                CompanyId = reader.GetInt32(1),
                                CompanyName = reader.GetString(2),
                                PaymentDate = DateTime.Parse(reader.GetString(3)),
                                Amount = reader.GetDecimal(4),
                                Period = reader.GetString(5),
                                PaymentMethod = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
                                IsConfirmed = reader.GetInt32(8) == 1
                            });
                        }
                    }
                }
            }
            return payments;
        }

        public void AddPayment(Payment payment)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Payments (CompanyId, CompanyName, PaymentDate, Amount, Period, 
                                PaymentMethod, Notes, IsConfirmed) 
                                VALUES (@CompanyId, @CompanyName, @PaymentDate, @Amount, @Period, 
                                @PaymentMethod, @Notes, @IsConfirmed)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    AddPaymentParameters(command, payment);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeletePayment(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Payments WHERE Id=@Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Expense CRUD
        public List<Expense> GetExpensesByPeriod(string period)
        {
            var expenses = new List<Expense>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Expenses WHERE Period=@Period";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Period", period);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            expenses.Add(new Expense
                            {
                                Id = reader.GetInt32(0),
                                ExpenseDate = DateTime.Parse(reader.GetString(1)),
                                Category = reader.GetString(2),
                                Amount = reader.GetDecimal(3),
                                Description = reader.GetString(4),
                                Period = reader.GetString(5),
                                Notes = reader.IsDBNull(6) ? null : reader.GetString(6)
                            });
                        }
                    }
                }
            }
            return expenses;
        }

        public void AddExpense(Expense expense)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Expenses (ExpenseDate, Category, Amount, Description, Period, Notes) 
                                VALUES (@ExpenseDate, @Category, @Amount, @Description, @Period, @Notes)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    AddExpenseParameters(command, expense);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteExpense(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Expenses WHERE Id=@Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Helper methods
        private void AddCompanyParameters(SQLiteCommand command, Company company)
        {
            command.Parameters.AddWithValue("@Name", company.Name);
            command.Parameters.AddWithValue("@Type", company.Type);
            command.Parameters.AddWithValue("@ContractDate", company.ContractDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@MonthlyFee", company.MonthlyFee);
            command.Parameters.AddWithValue("@ContactPerson", (object)company.ContactPerson ?? DBNull.Value);
            command.Parameters.AddWithValue("@PhoneNumber", (object)company.PhoneNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object)company.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@Notes", (object)company.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", company.IsActive ? 1 : 0);
        }

        private void AddPaymentParameters(SQLiteCommand command, Payment payment)
        {
            command.Parameters.AddWithValue("@CompanyId", payment.CompanyId);
            command.Parameters.AddWithValue("@CompanyName", payment.CompanyName);
            command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Amount", payment.Amount);
            command.Parameters.AddWithValue("@Period", payment.Period);
            command.Parameters.AddWithValue("@PaymentMethod", (object)payment.PaymentMethod ?? DBNull.Value);
            command.Parameters.AddWithValue("@Notes", (object)payment.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsConfirmed", payment.IsConfirmed ? 1 : 0);
        }

        private void AddExpenseParameters(SQLiteCommand command, Expense expense)
        {
            command.Parameters.AddWithValue("@ExpenseDate", expense.ExpenseDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Category", expense.Category);
            command.Parameters.AddWithValue("@Amount", expense.Amount);
            command.Parameters.AddWithValue("@Description", expense.Description);
            command.Parameters.AddWithValue("@Period", expense.Period);
            command.Parameters.AddWithValue("@Notes", (object)expense.Notes ?? DBNull.Value);
        }

        private void ExecuteNonQuery(string query, SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}

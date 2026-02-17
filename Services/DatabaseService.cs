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

                // NotificationSettings 테이블
                string createNotificationSettingsTable = @"
                    CREATE TABLE IF NOT EXISTS NotificationSettings (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        EnableSmsNotifications INTEGER NOT NULL DEFAULT 1,
                        EnableEmailNotifications INTEGER NOT NULL DEFAULT 1,
                        UnpaidEarlyMonth INTEGER NOT NULL DEFAULT 1,
                        UnpaidMidMonth INTEGER NOT NULL DEFAULT 1,
                        UnpaidEndMonth INTEGER NOT NULL DEFAULT 1,
                        RentWeekBefore INTEGER NOT NULL DEFAULT 1,
                        RentThreeDaysBefore INTEGER NOT NULL DEFAULT 1,
                        RentDueDate INTEGER NOT NULL DEFAULT 1,
                        SmsApiKey TEXT,
                        SmsApiSecret TEXT,
                        SmsSenderNumber TEXT,
                        EmailSmtpServer TEXT,
                        EmailSmtpPort TEXT,
                        EmailAddress TEXT,
                        EmailPassword TEXT,
                        EmailSenderName TEXT
                    )";

                // NotificationLogs 테이블
                string createNotificationLogsTable = @"
                    CREATE TABLE IF NOT EXISTS NotificationLogs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SentDate TEXT NOT NULL,
                        NotificationType TEXT NOT NULL,
                        Category TEXT NOT NULL,
                        CompanyId INTEGER NOT NULL,
                        CompanyName TEXT NOT NULL,
                        Recipient TEXT NOT NULL,
                        Message TEXT NOT NULL,
                        IsSuccess INTEGER NOT NULL,
                        ErrorMessage TEXT
                    )";

                ExecuteNonQuery(createCompaniesTable, connection);
                ExecuteNonQuery(createPaymentsTable, connection);
                ExecuteNonQuery(createExpensesTable, connection);
                ExecuteNonQuery(createNotificationSettingsTable, connection);
                ExecuteNonQuery(createNotificationLogsTable, connection);
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

        // NotificationSettings CRUD
        public NotificationSettings GetNotificationSettings()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM NotificationSettings LIMIT 1";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new NotificationSettings
                        {
                            Id = reader.GetInt32(0),
                            EnableSmsNotifications = reader.GetInt32(1) == 1,
                            EnableEmailNotifications = reader.GetInt32(2) == 1,
                            UnpaidEarlyMonth = reader.GetInt32(3) == 1,
                            UnpaidMidMonth = reader.GetInt32(4) == 1,
                            UnpaidEndMonth = reader.GetInt32(5) == 1,
                            RentWeekBefore = reader.GetInt32(6) == 1,
                            RentThreeDaysBefore = reader.GetInt32(7) == 1,
                            RentDueDate = reader.GetInt32(8) == 1,
                            SmsApiKey = reader.IsDBNull(9) ? null : reader.GetString(9),
                            SmsApiSecret = reader.IsDBNull(10) ? null : reader.GetString(10),
                            SmsSenderNumber = reader.IsDBNull(11) ? null : reader.GetString(11),
                            EmailSmtpServer = reader.IsDBNull(12) ? null : reader.GetString(12),
                            EmailSmtpPort = reader.IsDBNull(13) ? null : reader.GetString(13),
                            EmailAddress = reader.IsDBNull(14) ? null : reader.GetString(14),
                            EmailPassword = reader.IsDBNull(15) ? null : reader.GetString(15),
                            EmailSenderName = reader.IsDBNull(16) ? null : reader.GetString(16)
                        };
                    }
                }
            }
            // 설정이 없으면 기본값 반환
            return new NotificationSettings();
        }

        public void SaveNotificationSettings(NotificationSettings settings)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                // 기존 설정 확인
                string checkQuery = "SELECT COUNT(*) FROM NotificationSettings";
                int count = 0;
                using (var checkCommand = new SQLiteCommand(checkQuery, connection))
                {
                    count = Convert.ToInt32(checkCommand.ExecuteScalar());
                }

                string query;
                if (count == 0)
                {
                    // Insert
                    query = @"INSERT INTO NotificationSettings (
                        EnableSmsNotifications, EnableEmailNotifications,
                        UnpaidEarlyMonth, UnpaidMidMonth, UnpaidEndMonth,
                        RentWeekBefore, RentThreeDaysBefore, RentDueDate,
                        SmsApiKey, SmsApiSecret, SmsSenderNumber,
                        EmailSmtpServer, EmailSmtpPort, EmailAddress, EmailPassword, EmailSenderName
                    ) VALUES (
                        @EnableSms, @EnableEmail,
                        @UnpaidEarly, @UnpaidMid, @UnpaidEnd,
                        @RentWeek, @RentThree, @RentDue,
                        @SmsKey, @SmsSecret, @SmsSender,
                        @EmailServer, @EmailPort, @EmailAddr, @EmailPass, @EmailName
                    )";
                }
                else
                {
                    // Update
                    query = @"UPDATE NotificationSettings SET
                        EnableSmsNotifications=@EnableSms, EnableEmailNotifications=@EnableEmail,
                        UnpaidEarlyMonth=@UnpaidEarly, UnpaidMidMonth=@UnpaidMid, UnpaidEndMonth=@UnpaidEnd,
                        RentWeekBefore=@RentWeek, RentThreeDaysBefore=@RentThree, RentDueDate=@RentDue,
                        SmsApiKey=@SmsKey, SmsApiSecret=@SmsSecret, SmsSenderNumber=@SmsSender,
                        EmailSmtpServer=@EmailServer, EmailSmtpPort=@EmailPort, 
                        EmailAddress=@EmailAddr, EmailPassword=@EmailPass, EmailSenderName=@EmailName";
                }

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EnableSms", settings.EnableSmsNotifications ? 1 : 0);
                    command.Parameters.AddWithValue("@EnableEmail", settings.EnableEmailNotifications ? 1 : 0);
                    command.Parameters.AddWithValue("@UnpaidEarly", settings.UnpaidEarlyMonth ? 1 : 0);
                    command.Parameters.AddWithValue("@UnpaidMid", settings.UnpaidMidMonth ? 1 : 0);
                    command.Parameters.AddWithValue("@UnpaidEnd", settings.UnpaidEndMonth ? 1 : 0);
                    command.Parameters.AddWithValue("@RentWeek", settings.RentWeekBefore ? 1 : 0);
                    command.Parameters.AddWithValue("@RentThree", settings.RentThreeDaysBefore ? 1 : 0);
                    command.Parameters.AddWithValue("@RentDue", settings.RentDueDate ? 1 : 0);
                    command.Parameters.AddWithValue("@SmsKey", (object)settings.SmsApiKey ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SmsSecret", (object)settings.SmsApiSecret ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SmsSender", (object)settings.SmsSenderNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EmailServer", (object)settings.EmailSmtpServer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EmailPort", (object)settings.EmailSmtpPort ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EmailAddr", (object)settings.EmailAddress ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EmailPass", (object)settings.EmailPassword ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EmailName", (object)settings.EmailSenderName ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        // NotificationLog CRUD
        public void AddNotificationLog(NotificationLog log)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO NotificationLogs (
                    SentDate, NotificationType, Category, CompanyId, CompanyName,
                    Recipient, Message, IsSuccess, ErrorMessage
                ) VALUES (
                    @SentDate, @NotificationType, @Category, @CompanyId, @CompanyName,
                    @Recipient, @Message, @IsSuccess, @ErrorMessage
                )";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SentDate", log.SentDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@NotificationType", log.NotificationType);
                    command.Parameters.AddWithValue("@Category", log.Category);
                    command.Parameters.AddWithValue("@CompanyId", log.CompanyId);
                    command.Parameters.AddWithValue("@CompanyName", log.CompanyName);
                    command.Parameters.AddWithValue("@Recipient", log.Recipient);
                    command.Parameters.AddWithValue("@Message", log.Message);
                    command.Parameters.AddWithValue("@IsSuccess", log.IsSuccess ? 1 : 0);
                    command.Parameters.AddWithValue("@ErrorMessage", (object)log.ErrorMessage ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<NotificationLog> GetNotificationLogs(int limit = 100)
        {
            var logs = new List<NotificationLog>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM NotificationLogs ORDER BY SentDate DESC LIMIT {limit}";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(new NotificationLog
                        {
                            Id = reader.GetInt32(0),
                            SentDate = DateTime.Parse(reader.GetString(1)),
                            NotificationType = reader.GetString(2),
                            Category = reader.GetString(3),
                            CompanyId = reader.GetInt32(4),
                            CompanyName = reader.GetString(5),
                            Recipient = reader.GetString(6),
                            Message = reader.GetString(7),
                            IsSuccess = reader.GetInt32(8) == 1,
                            ErrorMessage = reader.IsDBNull(9) ? null : reader.GetString(9)
                        });
                    }
                }
            }
            return logs;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OfficeManagerWPF.Models;

namespace OfficeManagerWPF.Services
{
    public class NotificationScheduler
    {
        private readonly DatabaseService _dbService;
        private readonly SmsService _smsService;
        private readonly EmailService _emailService;
        private readonly NotificationSettings _settings;
        private Timer _timer;

        public NotificationScheduler(
            DatabaseService dbService,
            NotificationSettings settings)
        {
            _dbService = dbService;
            _settings = settings;
            _smsService = new SmsService(settings);
            _emailService = new EmailService(settings);
        }

        public void Start()
        {
            // 매일 오전 9시에 알림 체크
            var now = DateTime.Now;
            var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0);
            
            if (now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            var timeUntilScheduled = scheduledTime - now;
            
            _timer = new Timer(
                async _ => await CheckAndSendNotifications(),
                null,
                timeUntilScheduled,
                TimeSpan.FromDays(1) // 24시간마다 반복
            );
        }

        public void Stop()
        {
            _timer?.Dispose();
        }

        private async Task CheckAndSendNotifications()
        {
            try
            {
                var today = DateTime.Now;
                var currentPeriod = today.ToString("yyyy-MM");

                // 미수금 알림 체크
                await CheckUnpaidNotifications(today, currentPeriod);

                // 월세 납부 알림 체크
                await CheckRentReminderNotifications(today);
            }
            catch (Exception ex)
            {
                // 로그 기록
                System.Diagnostics.Debug.WriteLine($"알림 체크 오류: {ex.Message}");
            }
        }

        private async Task CheckUnpaidNotifications(DateTime today, string currentPeriod)
        {
            var day = today.Day;
            bool shouldSend = false;

            // 월초 (1-5일)
            if (_settings.UnpaidEarlyMonth && day >= 1 && day <= 5)
                shouldSend = true;
            
            // 중순 (13-17일)
            if (_settings.UnpaidMidMonth && day >= 13 && day <= 17)
                shouldSend = true;
            
            // 월말 (25-30일)
            if (_settings.UnpaidEndMonth && day >= 25 && day <= DateTime.DaysInMonth(today.Year, today.Month))
                shouldSend = true;

            if (!shouldSend) return;

            // 미수금 업체 찾기
            var allCompanies = _dbService.GetAllCompanies().Where(c => c.IsActive).ToList();
            var payments = _dbService.GetPaymentsByPeriod(currentPeriod);
            var paidCompanyIds = payments.Select(p => p.CompanyId).ToHashSet();

            var unpaidCompanies = allCompanies.Where(c => !paidCompanyIds.Contains(c.Id)).ToList();

            foreach (var company in unpaidCompanies)
            {
                // SMS 발송
                if (_settings.EnableSmsNotifications && !string.IsNullOrEmpty(company.PhoneNumber))
                {
                    var smsMessage = _smsService.GenerateUnpaidSmsMessage(
                        company, 
                        company.MonthlyFee, 
                        currentPeriod
                    );
                    
                    var smsSuccess = await _smsService.SendSmsAsync(company.PhoneNumber, smsMessage);
                    
                    _dbService.AddNotificationLog(new NotificationLog
                    {
                        SentDate = DateTime.Now,
                        NotificationType = "SMS",
                        Category = "미수금",
                        CompanyId = company.Id,
                        CompanyName = company.Name,
                        Recipient = company.PhoneNumber,
                        Message = smsMessage,
                        IsSuccess = smsSuccess
                    });
                }

                // 이메일 발송
                if (_settings.EnableEmailNotifications && !string.IsNullOrEmpty(company.Email))
                {
                    var emailBody = _emailService.GenerateUnpaidEmailBody(
                        company, 
                        company.MonthlyFee, 
                        currentPeriod
                    );
                    
                    var emailSuccess = await _emailService.SendEmailAsync(
                        company.Email, 
                        "[Office Manager] 미수금 납부 안내", 
                        emailBody
                    );
                    
                    _dbService.AddNotificationLog(new NotificationLog
                    {
                        SentDate = DateTime.Now,
                        NotificationType = "Email",
                        Category = "미수금",
                        CompanyId = company.Id,
                        CompanyName = company.Name,
                        Recipient = company.Email,
                        Message = "미수금 납부 안내 이메일",
                        IsSuccess = emailSuccess
                    });
                }
            }
        }

        private async Task CheckRentReminderNotifications(DateTime today)
        {
            var allCompanies = _dbService.GetAllCompanies().Where(c => c.IsActive).ToList();

            foreach (var company in allCompanies)
            {
                // 계약일 기준으로 월세 납부일 계산
                var dueDay = company.ContractDate.Day;
                var dueDate = new DateTime(today.Year, today.Month, 
                    Math.Min(dueDay, DateTime.DaysInMonth(today.Year, today.Month)));

                var daysUntilDue = (dueDate - today).Days;

                bool shouldSend = false;
                int daysBefore = 0;

                // 7일 전
                if (_settings.RentWeekBefore && daysUntilDue == 7)
                {
                    shouldSend = true;
                    daysBefore = 7;
                }
                
                // 3일 전
                if (_settings.RentThreeDaysBefore && daysUntilDue == 3)
                {
                    shouldSend = true;
                    daysBefore = 3;
                }
                
                // 당일
                if (_settings.RentDueDate && daysUntilDue == 0)
                {
                    shouldSend = true;
                    daysBefore = 0;
                }

                if (!shouldSend) continue;

                // SMS 발송
                if (_settings.EnableSmsNotifications && !string.IsNullOrEmpty(company.PhoneNumber))
                {
                    var smsMessage = _smsService.GenerateRentReminderSmsMessage(
                        company, 
                        dueDate, 
                        daysBefore
                    );
                    
                    var smsSuccess = await _smsService.SendSmsAsync(company.PhoneNumber, smsMessage);
                    
                    _dbService.AddNotificationLog(new NotificationLog
                    {
                        SentDate = DateTime.Now,
                        NotificationType = "SMS",
                        Category = "월세납부",
                        CompanyId = company.Id,
                        CompanyName = company.Name,
                        Recipient = company.PhoneNumber,
                        Message = smsMessage,
                        IsSuccess = smsSuccess
                    });
                }

                // 이메일 발송
                if (_settings.EnableEmailNotifications && !string.IsNullOrEmpty(company.Email))
                {
                    var emailBody = _emailService.GenerateRentReminderEmailBody(
                        company, 
                        dueDate, 
                        daysBefore
                    );
                    
                    var emailSuccess = await _emailService.SendEmailAsync(
                        company.Email, 
                        "[Office Manager] 월세 납부 안내", 
                        emailBody
                    );
                    
                    _dbService.AddNotificationLog(new NotificationLog
                    {
                        SentDate = DateTime.Now,
                        NotificationType = "Email",
                        Category = "월세납부",
                        CompanyId = company.Id,
                        CompanyName = company.Name,
                        Recipient = company.Email,
                        Message = "월세 납부 안내 이메일",
                        IsSuccess = emailSuccess
                    });
                }
            }
        }
    }
}

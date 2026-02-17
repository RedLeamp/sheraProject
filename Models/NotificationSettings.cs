using System;

namespace OfficeManagerWPF.Models
{
    public class NotificationSettings
    {
        public int Id { get; set; }
        public bool EnableSmsNotifications { get; set; }
        public bool EnableEmailNotifications { get; set; }
        
        // 미수금 알림 설정 (월 3회)
        public bool UnpaidEarlyMonth { get; set; }  // 월초 (1-5일)
        public bool UnpaidMidMonth { get; set; }    // 중순 (13-17일)
        public bool UnpaidEndMonth { get; set; }    // 월말 (25-30일)
        
        // 월세 납부 알림 (3회)
        public bool RentWeekBefore { get; set; }    // 7일 전
        public bool RentThreeDaysBefore { get; set; } // 3일 전
        public bool RentDueDate { get; set; }       // 당일
        
        // SMS API 설정
        public string SmsApiKey { get; set; }
        public string SmsApiSecret { get; set; }
        public string SmsSenderNumber { get; set; }
        
        // 이메일 설정
        public string EmailSmtpServer { get; set; }
        public string EmailSmtpPort { get; set; }
        public string EmailAddress { get; set; }
        public string EmailPassword { get; set; }
        public string EmailSenderName { get; set; }

        public NotificationSettings()
        {
            EnableSmsNotifications = true;
            EnableEmailNotifications = true;
            UnpaidEarlyMonth = true;
            UnpaidMidMonth = true;
            UnpaidEndMonth = true;
            RentWeekBefore = true;
            RentThreeDaysBefore = true;
            RentDueDate = true;
            EmailSmtpPort = "587";
        }
    }
}

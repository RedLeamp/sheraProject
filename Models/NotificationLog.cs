using System;

namespace OfficeManagerWPF.Models
{
    public class NotificationLog
    {
        public int Id { get; set; }
        public DateTime SentDate { get; set; }
        public string NotificationType { get; set; } // "SMS", "Email"
        public string Category { get; set; } // "미수금", "월세납부"
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Recipient { get; set; } // 전화번호 또는 이메일
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}

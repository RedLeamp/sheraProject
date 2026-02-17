using System;

namespace OfficeManagerWPF.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Period { get; set; } // "yyyy-MM" 형식
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
        public bool IsConfirmed { get; set; }

        public Payment()
        {
            IsConfirmed = true;
        }
    }
}

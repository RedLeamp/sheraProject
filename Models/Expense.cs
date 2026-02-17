using System;

namespace OfficeManagerWPF.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Category { get; set; } // "임대료", "전기세", "수도세", "인건비", "기타"
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Period { get; set; } // "yyyy-MM" 형식
        public string Notes { get; set; }
    }
}

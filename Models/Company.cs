using System;

namespace OfficeManagerWPF.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // "상주" or "비상주"
        public DateTime ContractDate { get; set; }
        public decimal MonthlyFee { get; set; }
        public string ContactPerson { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }

        public Company()
        {
            IsActive = true;
        }
    }
}

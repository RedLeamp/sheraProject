using System;
using System.Collections.Generic;

namespace OfficeManagerWPF.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // "상주" or "비상주" (기존 필드 유지)
        public string Status { get; set; } // "상주업체", "비상주업체", "폐업업체", "예치금반환업체"
        public DateTime ContractDate { get; set; }
        public decimal MonthlyFee { get; set; }
        public string ContactPerson { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
        public string Location { get; set; } // 지역 (남양, 향남 등)
        public bool IsActive { get; set; }

        // 입금 관련 필드
        public DateTime? LastPaymentDate { get; set; } // 마지막 입금일
        public decimal TotalPayments { get; set; } // 총 입금액 (현재 기간)
        public decimal UnpaidAmount { get; set; } // 미수금액
        public int PaymentCount { get; set; } // 입금 횟수
        public string PaymentStatus { get; set; } // 입금 상태: "정상", "지연", "미납"

        public Company()
        {
            IsActive = true;
            Status = "상주업체"; // 기본값
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using OfficeManagerWPF.Models;

namespace OfficeManagerWPF.Services
{
    public class ExcelService
    {
        public ExcelService()
        {
            // 기본 생성자 (DatabaseService 없이)
        }

        #region 업체 Excel 입출력

        public void ExportCompaniesToExcel(List<Company> companies, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("업체목록");
                
                // 헤더
                sheet.Cell(1, 1).Value = "업체ID";
                sheet.Cell(1, 2).Value = "업체명";
                sheet.Cell(1, 3).Value = "구분";
                sheet.Cell(1, 4).Value = "계약일자";
                sheet.Cell(1, 5).Value = "월이용료";
                sheet.Cell(1, 6).Value = "담당자";
                sheet.Cell(1, 7).Value = "연락처";
                sheet.Cell(1, 8).Value = "이메일";
                sheet.Cell(1, 9).Value = "비고";
                sheet.Cell(1, 10).Value = "활성상태";

                // 헤더 스타일
                var headerRange = sheet.Range(1, 1, 1, 10);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                // 데이터
                int row = 2;
                foreach (var company in companies)
                {
                    sheet.Cell(row, 1).Value = company.Id;
                    sheet.Cell(row, 2).Value = company.Name;
                    sheet.Cell(row, 3).Value = company.Type;
                    sheet.Cell(row, 4).Value = company.ContractDate.ToString("yyyy-MM-dd");
                    sheet.Cell(row, 5).Value = company.MonthlyFee;
                    sheet.Cell(row, 6).Value = company.ContactPerson ?? "";
                    sheet.Cell(row, 7).Value = company.PhoneNumber ?? "";
                    sheet.Cell(row, 8).Value = company.Email ?? "";
                    sheet.Cell(row, 9).Value = company.Notes ?? "";
                    sheet.Cell(row, 10).Value = company.IsActive ? "활성" : "비활성";
                    row++;
                }

                sheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        public List<Company> ImportCompaniesFromExcel(string filePath)
        {
            var companies = new List<Company>();
            
            using (var workbook = new XLWorkbook(filePath))
            {
                var sheet = workbook.Worksheet(1);
                var lastRow = sheet.LastRowUsed().RowNumber();
                
                for (int row = 2; row <= lastRow; row++)
                {
                    try
                    {
                        var company = new Company
                        {
                            Name = sheet.Cell(row, 2).GetString(),
                            Type = sheet.Cell(row, 3).GetString(),
                            ContractDate = DateTime.Parse(sheet.Cell(row, 4).GetString()),
                            MonthlyFee = sheet.Cell(row, 5).GetValue<decimal>(),
                            ContactPerson = sheet.Cell(row, 6).GetString(),
                            PhoneNumber = sheet.Cell(row, 7).GetString(),
                            Email = sheet.Cell(row, 8).GetString(),
                            Notes = sheet.Cell(row, 9).GetString(),
                            IsActive = sheet.Cell(row, 10).GetString() == "활성"
                        };
                        companies.Add(company);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"행 {row} 가져오기 실패: {ex.Message}");
                    }
                }
            }
            
            return companies;
        }

        #endregion

        #region 입금 Excel 입출력

        public void ExportPaymentsToExcel(List<Payment> payments, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("입금내역");
                
                // 헤더
                sheet.Cell(1, 1).Value = "입금ID";
                sheet.Cell(1, 2).Value = "업체ID";
                sheet.Cell(1, 3).Value = "업체명";
                sheet.Cell(1, 4).Value = "입금일자";
                sheet.Cell(1, 5).Value = "입금액";
                sheet.Cell(1, 6).Value = "입금기간";
                sheet.Cell(1, 7).Value = "결제방법";
                sheet.Cell(1, 8).Value = "비고";
                sheet.Cell(1, 9).Value = "확인여부";

                // 헤더 스타일
                var headerRange = sheet.Range(1, 1, 1, 9);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;

                // 데이터
                int row = 2;
                foreach (var payment in payments)
                {
                    sheet.Cell(row, 1).Value = payment.Id;
                    sheet.Cell(row, 2).Value = payment.CompanyId;
                    sheet.Cell(row, 3).Value = payment.CompanyName;
                    sheet.Cell(row, 4).Value = payment.PaymentDate.ToString("yyyy-MM-dd");
                    sheet.Cell(row, 5).Value = payment.Amount;
                    sheet.Cell(row, 6).Value = payment.Period;
                    sheet.Cell(row, 7).Value = payment.PaymentMethod ?? "";
                    sheet.Cell(row, 8).Value = payment.Notes ?? "";
                    sheet.Cell(row, 9).Value = payment.IsConfirmed ? "확인" : "미확인";
                    row++;
                }

                sheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        public List<Payment> ImportPaymentsFromExcel(string filePath)
        {
            var payments = new List<Payment>();
            
            using (var workbook = new XLWorkbook(filePath))
            {
                var sheet = workbook.Worksheet(1);
                var lastRow = sheet.LastRowUsed().RowNumber();
                
                for (int row = 2; row <= lastRow; row++)
                {
                    try
                    {
                        var payment = new Payment
                        {
                            CompanyId = sheet.Cell(row, 2).GetValue<int>(),
                            CompanyName = sheet.Cell(row, 3).GetString(),
                            PaymentDate = DateTime.Parse(sheet.Cell(row, 4).GetString()),
                            Amount = sheet.Cell(row, 5).GetValue<decimal>(),
                            Period = sheet.Cell(row, 6).GetString(),
                            PaymentMethod = sheet.Cell(row, 7).GetString(),
                            Notes = sheet.Cell(row, 8).GetString(),
                            IsConfirmed = sheet.Cell(row, 9).GetString() == "확인"
                        };
                        payments.Add(payment);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"행 {row} 가져오기 실패: {ex.Message}");
                    }
                }
            }
            
            return payments;
        }

        #endregion

        #region 지출 Excel 입출력

        public void ExportExpensesToExcel(List<Expense> expenses, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("지출내역");
                
                // 헤더
                sheet.Cell(1, 1).Value = "지출ID";
                sheet.Cell(1, 2).Value = "지출일자";
                sheet.Cell(1, 3).Value = "카테고리";
                sheet.Cell(1, 4).Value = "금액";
                sheet.Cell(1, 5).Value = "상세설명";
                sheet.Cell(1, 6).Value = "지출기간";
                sheet.Cell(1, 7).Value = "비고";

                // 헤더 스타일
                var headerRange = sheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightPink;

                // 데이터
                int row = 2;
                foreach (var expense in expenses)
                {
                    sheet.Cell(row, 1).Value = expense.Id;
                    sheet.Cell(row, 2).Value = expense.ExpenseDate.ToString("yyyy-MM-dd");
                    sheet.Cell(row, 3).Value = expense.Category;
                    sheet.Cell(row, 4).Value = expense.Amount;
                    sheet.Cell(row, 5).Value = expense.Description;
                    sheet.Cell(row, 6).Value = expense.Period;
                    sheet.Cell(row, 7).Value = expense.Notes ?? "";
                    row++;
                }

                sheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        public List<Expense> ImportExpensesFromExcel(string filePath)
        {
            var expenses = new List<Expense>();
            
            using (var workbook = new XLWorkbook(filePath))
            {
                var sheet = workbook.Worksheet(1);
                var lastRow = sheet.LastRowUsed().RowNumber();
                
                for (int row = 2; row <= lastRow; row++)
                {
                    try
                    {
                        var expense = new Expense
                        {
                            ExpenseDate = DateTime.Parse(sheet.Cell(row, 2).GetString()),
                            Category = sheet.Cell(row, 3).GetString(),
                            Amount = sheet.Cell(row, 4).GetValue<decimal>(),
                            Description = sheet.Cell(row, 5).GetString(),
                            Period = sheet.Cell(row, 6).GetString(),
                            Notes = sheet.Cell(row, 7).GetString()
                        };
                        expenses.Add(expense);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"행 {row} 가져오기 실패: {ex.Message}");
                    }
                }
            }
            
            return expenses;
        }

        #endregion
    }
}

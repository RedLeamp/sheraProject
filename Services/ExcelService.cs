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

        #region 복잡한 Excel 파일 Import (남양/향남 임대내역 형식)

        /// <summary>
        /// 복잡한 Excel 파일 구조를 분석하여 업체/입금 데이터 추출
        /// 헤더가 2행에 있고, 데이터가 3행부터 시작하는 형식 지원
        /// </summary>
        public (List<Company> companies, List<Payment> payments) ImportComplexExcel(string filePath, string locationPrefix = "")
        {
            var companies = new List<Company>();
            var payments = new List<Payment>();
            
            try
            {
                // LoadOptions를 사용하여 읽기 전용 모드로 파일 열기
                var loadOptions = new LoadOptions
                {
                    RecalculateAllFormulas = false // 수식 재계산 비활성화로 성능 향상
                };
                
                using (var workbook = new XLWorkbook(filePath, loadOptions))
                {
                    // 모든 시트 처리 (1월, 2월 등)
                    foreach (var worksheet in workbook.Worksheets)
                    {
                        var sheetName = worksheet.Name;
                        
                        // "정산" 시트는 건너뛰기
                        if (sheetName.Contains("정산"))
                            continue;
                        
                        System.Diagnostics.Debug.WriteLine($"처리 중인 시트: {sheetName}");
                        
                        // 헤더 행 찾기 (보통 2행)
                        int headerRow = FindHeaderRow(worksheet);
                        if (headerRow == -1)
                        {
                            System.Diagnostics.Debug.WriteLine($"시트 {sheetName}에서 헤더를 찾을 수 없습니다.");
                            continue;
                        }
                        
                        // 헤더 매핑
                        var columnMapping = MapColumns(worksheet, headerRow);
                        
                        // 🔍 디버그: 헤더 매핑 결과 출력
                        System.Diagnostics.Debug.WriteLine($"헤더 행: {headerRow}, 매핑된 컬럼: {string.Join(", ", columnMapping.Select(kv => $"{kv.Key}={kv.Value}"))}");
                        
                        // 데이터 행 처리 (헤더 다음 행부터)
                        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? headerRow;
                        
                        // 현재 구분 (상주업체/비상주업체)
                        string currentSection = "";
                        
                        for (int row = headerRow + 1; row <= lastRow; row++)
                        {
                            try
                            {
                                // 전체 행에서 구분 체크 (첫 3개 열 검사)
                                bool isResidentSection = false;
                                bool isNonResidentSection = false;
                                bool isClosedSection = false;
                                bool isDepositRefundSection = false;
                                bool isMaintenanceSection = false; // 유지업체 - 무시
                                bool isNewSection = false; // 신규업체 - 무시
                                
                                for (int col = 1; col <= Math.Min(3, worksheet.LastColumnUsed()?.ColumnNumber() ?? 3); col++)
                                {
                                    var cell = worksheet.Cell(row, col);
                                    string cellValue = "";
                                    bool isMerged = false;
                                    
                                    // 병합된 셀 처리 - ClosedXML 방식
                                    try
                                    {
                                        // 셀 값 가져오기 시도
                                        cellValue = cell.Value.ToString().Trim();
                                        
                                        // 값이 비어있고, 병합된 셀일 가능성이 있는 경우
                                        if (string.IsNullOrEmpty(cellValue))
                                        {
                                            // 위쪽 셀 확인 (병합된 셀의 경우 위쪽 셀에 값이 있을 수 있음)
                                            if (row > 1)
                                            {
                                                var upperCell = worksheet.Cell(row - 1, col);
                                                var upperValue = upperCell.Value.ToString().Trim();
                                                
                                                // 위쪽 셀에 "상주업체" 등의 구분 값이 있으면 사용
                                                if (!string.IsNullOrEmpty(upperValue) && 
                                                    (upperValue.Contains("업체") || upperValue.Contains("상주") || upperValue.Contains("비상주")))
                                                {
                                                    cellValue = upperValue;
                                                    isMerged = true;
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        cellValue = "";
                                    }
                                    
                                    System.Diagnostics.Debug.WriteLine($"행 {row}, 열 {col}: '{cellValue}' (병합추정={isMerged})");
                                    
                                    // 유지업체/신규업체 체크 (무시할 섹션)
                                    if (cellValue.Contains("유지업체") || cellValue.Contains("유지 업체"))
                                    {
                                        isMaintenanceSection = true;
                                        break;
                                    }
                                    else if (cellValue.Contains("신규업체") || cellValue.Contains("신규 업체"))
                                    {
                                        isNewSection = true;
                                        break;
                                    }
                                    // 폐업업체
                                    else if (cellValue.Contains("폐업업체") || cellValue.Contains("폐업 업체"))
                                    {
                                        isClosedSection = true;
                                        break;
                                    }
                                    // 예치금반환업체
                                    else if (cellValue.Contains("예치금반환업체") || cellValue.Contains("예치금반환 업체") || 
                                             cellValue.Contains("예치금 반환업체") || cellValue.Contains("예치금 반환 업체"))
                                    {
                                        isDepositRefundSection = true;
                                        break;
                                    }
                                    // 상주업체
                                    else if (cellValue.Contains("상주업체") && !cellValue.Contains("비상주"))
                                    {
                                        isResidentSection = true;
                                        break;
                                    }
                                    // 비상주업체
                                    else if (cellValue.Contains("비상주업체") || cellValue.Contains("비상주 업체"))
                                    {
                                        isNonResidentSection = true;
                                        break;
                                    }
                                }
                                
                                // 유지업체/신규업체 섹션 - 무시 섹션으로 설정
                                if (isMaintenanceSection)
                                {
                                    currentSection = "IGNORE_MAINTENANCE"; // 무시 섹션으로 명시적 표시
                                    System.Diagnostics.Debug.WriteLine($"행 {row}: 유지업체 섹션 - 무시 시작");
                                    continue;
                                }
                                else if (isNewSection)
                                {
                                    currentSection = "IGNORE_NEW"; // 무시 섹션으로 명시적 표시
                                    System.Diagnostics.Debug.WriteLine($"행 {row}: 신규업체 섹션 - 무시 시작");
                                    continue;
                                }
                                // "상주업체" 구분 감지
                                else if (isResidentSection)
                                {
                                    // 번호 열(B열, 2번 열) 확인 - 실제 데이터 행인지 섹션 제목 행인지 구분
                                    var numberCell = worksheet.Cell(row, 2).Value.ToString().Trim();
                                    bool isHeaderRow = string.IsNullOrEmpty(numberCell) || numberCell == "번호";
                                    
                                    currentSection = "상주업체";
                                    
                                    if (isHeaderRow)
                                    {
                                        // 섹션 제목 행 - 건너뛰기
                                        System.Diagnostics.Debug.WriteLine($"행 {row}: 상주업체 섹션 시작 (제목 행) - 건너뛰기");
                                        continue;
                                    }
                                    else
                                    {
                                        // 실제 데이터 행 - 아래로 진행하여 데이터 처리
                                        System.Diagnostics.Debug.WriteLine($"행 {row}: 상주업체 데이터 행 (번호={numberCell}) - 처리 진행");
                                        // continue 하지 않고 아래로 진행
                                    }
                                }
                                // "비상주업체" 구분 감지
                                else if (isNonResidentSection)
                                {
                                    // 번호 열(B열, 2번 열) 확인
                                    var numberCell = worksheet.Cell(row, 2).Value.ToString().Trim();
                                    bool isHeaderRow = string.IsNullOrEmpty(numberCell) || numberCell == "번호";
                                    
                                    currentSection = "비상주업체";
                                    
                                    if (isHeaderRow)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"행 {row}: 비상주업체 섹션 시작 (제목 행) - 건너뛰기");
                                        continue;
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"행 {row}: 비상주업체 데이터 행 (번호={numberCell}) - 처리 진행");
                                        // continue 하지 않고 아래로 진행
                                    }
                                }
                                // "폐업업체" 구분 감지
                                else if (isClosedSection)
                                {
                                    // 번호 열(B열, 2번 열) 확인
                                    var numberCell = worksheet.Cell(row, 2).Value.ToString().Trim();
                                    bool isHeaderRow = string.IsNullOrEmpty(numberCell) || numberCell == "번호";
                                    
                                    currentSection = "폐업업체";
                                    
                                    if (isHeaderRow)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"행 {row}: 폐업업체 섹션 시작 (제목 행) - 건너뛰기");
                                        continue;
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"행 {row}: 폐업업체 데이터 행 (번호={numberCell}) - 처리 진행");
                                        // continue 하지 않고 아래로 진행
                                    }
                                }
                                // "예치금반환업체" 구분 감지
                                else if (isDepositRefundSection)
                                {
                                    // 번호 열(B열, 2번 열) 확인
                                    var numberCell = worksheet.Cell(row, 2).Value.ToString().Trim();
                                    bool isHeaderRow = string.IsNullOrEmpty(numberCell) || numberCell == "번호";
                                    
                                    currentSection = "예치금반환업체";
                                    
                                    if (isHeaderRow)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"행 {row}: 예치금반환업체 섹션 시작 (제목 행) - 건너뛰기");
                                        continue;
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"행 {row}: 예치금반환업체 데이터 행 (번호={numberCell}) - 처리 진행");
                                        // continue 하지 않고 아래로 진행
                                    }
                                }
                                
                                // "총 액" 체크 - 데이터 입력 종료 또는 다음 섹션으로
                                var firstCellValue = worksheet.Cell(row, 1).Value.ToString().Trim();
                                if (firstCellValue.Contains("총") || firstCellValue.Contains("합계"))
                                {
                                    System.Diagnostics.Debug.WriteLine($"행 {row}: 총액 행 감지 - 현재 섹션 종료");
                                    currentSection = ""; // 섹션 종료, 다음 섹션 대기
                                    continue;
                                }
                                
                                // 무시 섹션(유지업체/신규업체)에 있는 데이터는 건너뛰기
                                if (currentSection == "IGNORE_MAINTENANCE" || currentSection == "IGNORE_NEW")
                                {
                                    System.Diagnostics.Debug.WriteLine($"행 {row}: 무시 섹션 내 데이터 - 건너뛰기");
                                    continue;
                                }
                                
                                // 섹션이 설정되지 않았으면 건너뛰기
                                if (string.IsNullOrWhiteSpace(currentSection))
                                {
                                    System.Diagnostics.Debug.WriteLine($"행 {row}: 섹션이 설정되지 않음 - 건너뛰기 (첫 3열: '{worksheet.Cell(row, 1).Value}', '{worksheet.Cell(row, 2).Value}', '{worksheet.Cell(row, 3).Value}')");
                                    continue;
                                }
                                
                                // ✅ A열(첫 번째 열) 체크 - "유지업체" 또는 "신규업체"가 있는 행 전체 무시
                                var firstCell = worksheet.Cell(row, 1);
                                string firstColumnValue = "";
                                
                                // 병합된 셀 처리 - ClosedXML 방식
                                try
                                {
                                    firstColumnValue = firstCell.Value.ToString().Trim();
                                    
                                    // 값이 비어있고 병합된 셀일 가능성이 있는 경우
                                    if (string.IsNullOrEmpty(firstColumnValue) && row > 1)
                                    {
                                        // 위쪽 셀 확인
                                        var upperCell = worksheet.Cell(row - 1, 1);
                                        var upperValue = upperCell.Value.ToString().Trim();
                                        
                                        if (!string.IsNullOrEmpty(upperValue) && 
                                            (upperValue.Contains("업체") || upperValue.Contains("상주") || upperValue.Contains("비상주")))
                                        {
                                            firstColumnValue = upperValue;
                                        }
                                    }
                                }
                                catch
                                {
                                    firstColumnValue = "";
                                }
                                
                                if (firstColumnValue.Contains("유지업체") || firstColumnValue.Contains("신규업체"))
                                {
                                    System.Diagnostics.Debug.WriteLine($"행 {row}: A열에 '{firstColumnValue}' 감지 - 전체 행 무시");
                                    continue;
                                }
                                
                                var rowData = GetRowData(worksheet, row, columnMapping);
                                
                                // 🔍 디버그: 읽은 데이터 출력
                                System.Diagnostics.Debug.WriteLine($"행 {row}: A열='{firstColumnValue}', 업체명='{rowData.CompanyName}', 구분='{rowData.CompanyType}'");
                                
                                // 업체명이 비어있으면 건너뛰기
                                if (string.IsNullOrWhiteSpace(rowData.CompanyName))
                                {
                                    System.Diagnostics.Debug.WriteLine($"행 {row}: 업체명이 비어있음 - 건너뛰기");
                                    continue;
                                }
                                
                                // ✅ 업체명에 "유지업체" 또는 "신규업체"가 포함된 경우 무시
                                if (rowData.CompanyName.Contains("유지업체") || rowData.CompanyName.Contains("신규업체"))
                                {
                                    System.Diagnostics.Debug.WriteLine($"행 {row}: 업체명 '{rowData.CompanyName}' - 유지/신규업체 데이터 무시");
                                    continue;
                                }
                                
                                // 구분 필드에 상태 정보 저장 (상주업체, 비상주업체, 폐업업체, 예치금반환업체)
                                if (currentSection == "상주업체")
                                {
                                    rowData.CompanyType = "상주";
                                }
                                else if (currentSection == "비상주업체")
                                {
                                    rowData.CompanyType = "비상주";
                                }
                                else if (currentSection == "폐업업체")
                                {
                                    rowData.CompanyType = "폐업";
                                }
                                else if (currentSection == "예치금반환업체")
                                {
                                    rowData.CompanyType = "예치금반환";
                                }
                                
                                System.Diagnostics.Debug.WriteLine($"행 {row}: {rowData.CompanyName} ({rowData.CompanyType})");
                                
                                // Company 객체 생성
                                var company = CreateCompanyFromRow(rowData, locationPrefix);
                                if (company != null && !companies.Any(c => c.Name == company.Name && c.PhoneNumber == company.PhoneNumber))
                                {
                                    companies.Add(company);
                                }
                                
                                // Payment 객체 생성
                                var payment = CreatePaymentFromRow(rowData, sheetName);
                                if (payment != null)
                                {
                                    payments.Add(payment);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"행 {row} 처리 실패: {ex.Message}");
                            }
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"총 {companies.Count}개 업체, {payments.Count}개 입금 데이터 추출 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Excel 파일 읽기 실패: {ex.Message}");
                throw;
            }
            
            return (companies, payments);
        }
        
        private int FindHeaderRow(IXLWorksheet worksheet)
        {
            // 처음 10행 중에서 "업체명" 또는 "구분" 포함된 행 찾기
            for (int row = 1; row <= Math.Min(10, worksheet.LastRowUsed()?.RowNumber() ?? 10); row++)
            {
                var cell1 = worksheet.Cell(row, 1).Value.ToString();
                var cell3 = worksheet.Cell(row, 3).Value.ToString();
                
                if (cell1.Contains("구분") || cell3.Contains("업체명"))
                {
                    return row;
                }
            }
            return -1;
        }
        
        private Dictionary<string, int> MapColumns(IXLWorksheet worksheet, int headerRow)
        {
            var mapping = new Dictionary<string, int>();
            var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 30;
            
            for (int col = 1; col <= lastCol; col++)
            {
                var header = worksheet.Cell(headerRow, col).Value.ToString().Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    mapping[header] = col;
                }
            }
            
            return mapping;
        }
        
        private RowData GetRowData(IXLWorksheet worksheet, int row, Dictionary<string, int> columnMapping)
        {
            var data = new RowData();
            
            if (columnMapping.ContainsKey("업체명"))
                data.CompanyName = worksheet.Cell(row, columnMapping["업체명"]).Value.ToString().Trim();
            
            if (columnMapping.ContainsKey("계약자"))
                data.ContactPerson = worksheet.Cell(row, columnMapping["계약자"]).Value.ToString().Trim();
            
            if (columnMapping.ContainsKey("전화번호"))
                data.PhoneNumber = worksheet.Cell(row, columnMapping["전화번호"]).Value.ToString().Trim();
            
            if (columnMapping.ContainsKey("사업자등록증"))
                data.BusinessNumber = worksheet.Cell(row, columnMapping["사업자등록증"]).Value.ToString().Trim();
            
            if (columnMapping.ContainsKey("멜 주소"))
                data.Email = worksheet.Cell(row, columnMapping["멜 주소"]).Value.ToString().Trim();
            
            if (columnMapping.ContainsKey("최초계약일자"))
            {
                var cellValue = worksheet.Cell(row, columnMapping["최초계약일자"]).Value;
                if (cellValue.IsDateTime)
                    data.ContractDate = cellValue.GetDateTime();
                else if (DateTime.TryParse(cellValue.ToString(), out var parsedDate))
                    data.ContractDate = parsedDate;
            }
            
            if (columnMapping.ContainsKey("월임대료"))
            {
                var cellValue = worksheet.Cell(row, columnMapping["월임대료"]).Value;
                if (cellValue.IsNumber)
                    data.MonthlyFee = (decimal)cellValue.GetNumber();
                else if (decimal.TryParse(cellValue.ToString(), out var fee))
                    data.MonthlyFee = fee;
            }
            
            if (columnMapping.ContainsKey("입금일자"))
            {
                var cellValue = worksheet.Cell(row, columnMapping["입금일자"]).Value;
                if (cellValue.IsDateTime)
                    data.PaymentDate = cellValue.GetDateTime();
                else if (DateTime.TryParse(cellValue.ToString(), out var parsedDate))
                    data.PaymentDate = parsedDate;
            }
            
            if (columnMapping.ContainsKey("납입임대료"))
            {
                var cellValue = worksheet.Cell(row, columnMapping["납입임대료"]).Value;
                if (cellValue.IsNumber)
                    data.Amount = (decimal)cellValue.GetNumber();
                else if (decimal.TryParse(cellValue.ToString(), out var amount))
                    data.Amount = amount;
            }
            
            if (columnMapping.ContainsKey("법인/개인"))
            {
                var excelType = worksheet.Cell(row, columnMapping["법인/개인"]).GetString().Trim();
                // CompanyType이 이미 설정되어 있으면 (상주/비상주) 유지, 없으면 엑셀 값 사용
                if (string.IsNullOrWhiteSpace(data.CompanyType))
                    data.CompanyType = excelType;
            }
            else if (columnMapping.ContainsKey("개인/법인"))
            {
                var excelType = worksheet.Cell(row, columnMapping["개인/법인"]).GetString().Trim();
                if (string.IsNullOrWhiteSpace(data.CompanyType))
                    data.CompanyType = excelType;
            }
            else if (columnMapping.ContainsKey("구분"))
            {
                var excelType = worksheet.Cell(row, columnMapping["구분"]).GetString().Trim();
                if (string.IsNullOrWhiteSpace(data.CompanyType))
                    data.CompanyType = excelType;
            }
            
            if (columnMapping.ContainsKey("폐업서류접수여부 / 비    고"))
                data.Notes = worksheet.Cell(row, columnMapping["폐업서류접수여부 / 비    고"]).GetString().Trim();
            else if (columnMapping.ContainsKey("비고"))
                data.Notes = worksheet.Cell(row, columnMapping["비고"]).GetString().Trim();
            
            return data;
        }
        
        private Company CreateCompanyFromRow(RowData rowData, string locationPrefix)
        {
            if (string.IsNullOrWhiteSpace(rowData.CompanyName))
                return null;
            
            // Type 결정: 상주/비상주/폐업/예치금반환 (Excel의 "구분" 컬럼에서 가져옴)
            string companyType = "상주"; // 기본값
            string companyStatus = "상주"; // 기본 상태
            
            if (!string.IsNullOrWhiteSpace(rowData.CompanyType))
            {
                string typeStr = rowData.CompanyType.Trim();
                
                // 4가지 카테고리 분류
                if (typeStr.Contains("폐업"))
                {
                    companyType = "폐업";
                    companyStatus = "폐업";
                }
                else if (typeStr.Contains("예치금"))
                {
                    companyType = "예치금반환";
                    companyStatus = "예치금반환";
                }
                else if (typeStr.Contains("비상주") || typeStr.Equals("비입"))
                {
                    companyType = "비상주";
                    companyStatus = "비상주";
                }
                else if (typeStr.Contains("상주") || typeStr.Equals("법입"))
                {
                    companyType = "상주";
                    companyStatus = "상주";
                }
            }
            
            return new Company
            {
                Name = string.IsNullOrWhiteSpace(locationPrefix) ? rowData.CompanyName : $"[{locationPrefix}] {rowData.CompanyName}",
                Type = companyType,
                Status = companyStatus, // 추가: 상태 설정
                ContractDate = rowData.ContractDate ?? DateTime.Now,
                MonthlyFee = rowData.MonthlyFee,
                ContactPerson = rowData.ContactPerson,
                PhoneNumber = rowData.PhoneNumber,
                Email = rowData.Email,
                Notes = rowData.Notes,
                Location = locationPrefix,
                IsActive = companyStatus != "폐업" // 폐업업체는 비활성화
            };
        }
        
        private Payment CreatePaymentFromRow(RowData rowData, string period)
        {
            if (rowData.PaymentDate == null || rowData.Amount <= 0)
                return null;
            
            return new Payment
            {
                CompanyName = rowData.CompanyName,
                PaymentDate = rowData.PaymentDate.Value,
                Amount = rowData.Amount,
                Period = period,
                PaymentMethod = "계좌이체",
                Notes = rowData.Notes,
                IsConfirmed = true
            };
        }
        
        // 행 데이터를 담을 내부 클래스
        private class RowData
        {
            public string CompanyName { get; set; }
            public string ContactPerson { get; set; }
            public string PhoneNumber { get; set; }
            public string BusinessNumber { get; set; }
            public string Email { get; set; }
            public DateTime? ContractDate { get; set; }
            public decimal MonthlyFee { get; set; }
            public DateTime? PaymentDate { get; set; }
            public decimal Amount { get; set; }
            public string CompanyType { get; set; }
            public string Notes { get; set; }
        }

        #endregion
    }
}

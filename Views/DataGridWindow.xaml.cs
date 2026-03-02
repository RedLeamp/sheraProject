using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using OfficeManagerWPF.Models;
using OfficeManagerWPF.Services;

namespace OfficeManagerWPF.Views
{
    public partial class DataGridWindow : Window
    {
        private readonly DatabaseService _databaseService;
        private readonly ExcelService _excelService;

        public DataGridWindow()
        {
            InitializeComponent();
            SetWindowIcon();

            _databaseService = new DatabaseService();
            _excelService = new ExcelService();

            // 초기 데이터 로드
            LoadAllData();
        }

        private void SetWindowIcon()
        {
            try
            {
                var iconUri = new Uri("pack://application:,,,/Resources/app.ico", UriKind.Absolute);
                this.Icon = BitmapFrame.Create(iconUri);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"아이콘 로딩 실패: {ex.Message}");
            }
        }

        private void LoadAllData()
        {
            RefreshCompanies();
            RefreshPayments();
            RefreshExpenses();
        }

        #region 업체 데이터

        private void RefreshCompanies()
        {
            try
            {
                var companies = _databaseService.GetAllCompanies();
                CompaniesDataGrid.ItemsSource = companies;
                UpdateStatus($"업체 데이터 로드 완료 (총 {companies.Count}건)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"업체 데이터 로드 실패:\n{ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshCompanies_Click(object sender, RoutedEventArgs e)
        {
            RefreshCompanies();
        }

        private void ExportCompaniesExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel 파일 (*.xlsx)|*.xlsx",
                    FileName = $"업체목록_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var companies = _databaseService.GetAllCompanies();
                    _excelService.ExportCompaniesToExcel(companies, saveDialog.FileName);
                    
                    UpdateStatus($"Excel 내보내기 완료: {saveDialog.FileName}");
                    
                    MessageBox.Show($"Excel 파일로 내보내기 완료!\n\n파일: {saveDialog.FileName}", 
                        "완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel 내보내기 실패:\n{ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportCompaniesExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "Excel 파일 (*.xlsx)|*.xlsx|모든 파일 (*.*)|*.*"
                };

                if (openDialog.ShowDialog() == true)
                {
                    var result = MessageBox.Show(
                        "Excel 파일의 업체 데이터를 가져옵니다.\n기존 데이터는 유지되며, 새로운 데이터만 추가됩니다.\n\n계속하시겠습니까?",
                        "확인",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var companies = _excelService.ImportCompaniesFromExcel(openDialog.FileName);
                        
                        int addedCount = 0;
                        foreach (var company in companies)
                        {
                            _databaseService.AddCompany(company);
                            addedCount++;
                        }

                        RefreshCompanies();
                        UpdateStatus($"Excel 가져오기 완료: {addedCount}건 추가됨");
                        
                        MessageBox.Show($"Excel 파일에서 {addedCount}건의 업체 데이터를 가져왔습니다!", 
                            "완료", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel 가져오기 실패:\n{ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportComplexExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "Excel 파일 (*.xlsx)|*.xlsx|모든 파일 (*.*)|*.*",
                    Title = "남양/향남 임대내역 Excel 파일 선택"
                };

                if (openDialog.ShowDialog() == true)
                {
                    // 파일 이름에서 지역 정보 추출
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(openDialog.FileName);
                    string locationPrefix = "";
                    
                    if (fileName.Contains("남양"))
                        locationPrefix = "남양";
                    else if (fileName.Contains("향남"))
                        locationPrefix = "향남";

                    var result = MessageBox.Show(
                        $"임대내역 Excel 파일을 가져옵니다.\n\n" +
                        $"파일: {fileName}\n" +
                        $"지역: {(string.IsNullOrEmpty(locationPrefix) ? "미지정" : locationPrefix)}\n\n" +
                        $"• 모든 시트의 업체 정보를 추출합니다.\n" +
                        $"• 입금 내역도 함께 추출됩니다.\n" +
                        $"• 기존 데이터는 유지되며, 새로운 데이터만 추가됩니다.\n\n" +
                        $"계속하시겠습니까?",
                        "확인",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        UpdateStatus("임대내역 분석 중...");
                        
                        // Excel 파일에서 데이터 추출
                        var (companies, payments) = _excelService.ImportComplexExcel(openDialog.FileName, locationPrefix);
                        
                        int addedCompanies = 0;
                        int addedPayments = 0;
                        int skippedPayments = 0;
                        
                        // 업체명 → CompanyId 매핑 딕셔너리 생성
                        var companyIdMap = new Dictionary<string, int>();
                        
                        // 1단계: 업체 데이터 추가하고 ID 매핑
                        foreach (var company in companies)
                        {
                            try
                            {
                                _databaseService.AddCompany(company);
                                addedCompanies++;
                                
                                // 방금 추가한 업체의 ID 가져오기
                                var allCompanies = _databaseService.GetAllCompanies();
                                var addedCompany = allCompanies.FirstOrDefault(c => 
                                    c.Name == company.Name && 
                                    c.PhoneNumber == company.PhoneNumber);
                                
                                if (addedCompany != null)
                                {
                                    // 지역 태그 제거한 순수 업체명으로 매핑
                                    var cleanName = company.Name.Replace($"[{locationPrefix}] ", "");
                                    if (!companyIdMap.ContainsKey(cleanName))
                                    {
                                        companyIdMap[cleanName] = addedCompany.Id;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"업체 추가 실패 ({company.Name}): {ex.Message}");
                            }
                        }
                        
                        // 2단계: 입금 데이터에 CompanyId 설정하고 추가
                        foreach (var payment in payments)
                        {
                            try
                            {
                                // 업체명으로 CompanyId 찾기
                                if (companyIdMap.ContainsKey(payment.CompanyName))
                                {
                                    payment.CompanyId = companyIdMap[payment.CompanyName];
                                    _databaseService.AddPayment(payment);
                                    addedPayments++;
                                }
                                else
                                {
                                    // CompanyId를 찾을 수 없으면 기존 업체 검색
                                    var existingCompany = _databaseService.GetAllCompanies()
                                        .FirstOrDefault(c => c.Name.Contains(payment.CompanyName));
                                    
                                    if (existingCompany != null)
                                    {
                                        payment.CompanyId = existingCompany.Id;
                                        _databaseService.AddPayment(payment);
                                        addedPayments++;
                                    }
                                    else
                                    {
                                        skippedPayments++;
                                        System.Diagnostics.Debug.WriteLine($"입금 추가 건너뜀 (업체를 찾을 수 없음): {payment.CompanyName}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"입금 추가 실패 ({payment.CompanyName}): {ex.Message}");
                                skippedPayments++;
                            }
                        }

                        RefreshCompanies();
                        RefreshPayments();
                        
                        UpdateStatus($"임대내역 가져오기 완료: 업체 {addedCompanies}건, 입금 {addedPayments}건 추가됨");
                        
                        var statusMessage = $"임대내역 Excel 파일 가져오기 완료!\n\n" +
                            $"• 업체 정보: {addedCompanies}건 추가\n" +
                            $"• 입금 내역: {addedPayments}건 추가\n";
                        
                        if (skippedPayments > 0)
                        {
                            statusMessage += $"• 건너뛴 입금: {skippedPayments}건 (업체 미매칭)\n";
                        }
                        
                        statusMessage += $"\n총 {companies.Count}개 업체, {payments.Count}개 입금 데이터를 분석했습니다.";
                        
                        MessageBox.Show(statusMessage, "완료", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"임대내역 가져오기 실패:\n{ex.Message}\n\n{ex.StackTrace}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 입금 데이터

        private void RefreshPayments()
        {
            try
            {
                var currentPeriod = DateTime.Now.ToString("yyyy-MM");
                var payments = _databaseService.GetPaymentsByPeriod(currentPeriod);
                PaymentsDataGrid.ItemsSource = payments;
                UpdateStatus($"입금 데이터 로드 완료 (총 {payments.Count}건)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"입금 데이터 로드 실패:\n{ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshPayments_Click(object sender, RoutedEventArgs e)
        {
            RefreshPayments();
        }

        private void ExportPaymentsExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel 파일 (*.xlsx)|*.xlsx",
                    FileName = $"입금내역_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var currentPeriod = DateTime.Now.ToString("yyyy-MM");
                    var payments = _databaseService.GetPaymentsByPeriod(currentPeriod);
                    _excelService.ExportPaymentsToExcel(payments, saveDialog.FileName);
                    
                    UpdateStatus($"Excel 내보내기 완료: {saveDialog.FileName}");
                    
                    MessageBox.Show($"Excel 파일로 내보내기 완료!\n\n파일: {saveDialog.FileName}", 
                        "완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel 내보내기 실패:\n{ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportPaymentsExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "Excel 파일 (*.xlsx)|*.xlsx|모든 파일 (*.*)|*.*"
                };

                if (openDialog.ShowDialog() == true)
                {
                    var result = MessageBox.Show(
                        "Excel 파일의 입금 데이터를 가져옵니다.\n\n계속하시겠습니까?",
                        "확인",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var payments = _excelService.ImportPaymentsFromExcel(openDialog.FileName);
                        
                        int addedCount = 0;
                        foreach (var payment in payments)
                        {
                            _databaseService.AddPayment(payment);
                            addedCount++;
                        }

                        RefreshPayments();
                        UpdateStatus($"Excel 가져오기 완료: {addedCount}건 추가됨");
                        
                        MessageBox.Show($"Excel 파일에서 {addedCount}건의 입금 데이터를 가져왔습니다!", 
                            "완료", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel 가져오기 실패:\n{ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 지출 데이터

        private void RefreshExpenses()
        {
            try
            {
                var currentPeriod = DateTime.Now.ToString("yyyy-MM");
                var expenses = _databaseService.GetExpensesByPeriod(currentPeriod);
                ExpensesDataGrid.ItemsSource = expenses;
                UpdateStatus($"지출 데이터 로드 완료 (총 {expenses.Count}건)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"지출 데이터 로드 실패:\n{ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshExpenses_Click(object sender, RoutedEventArgs e)
        {
            RefreshExpenses();
        }

        private void ExportExpensesExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel 파일 (*.xlsx)|*.xlsx",
                    FileName = $"지출내역_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var currentPeriod = DateTime.Now.ToString("yyyy-MM");
                    var expenses = _databaseService.GetExpensesByPeriod(currentPeriod);
                    _excelService.ExportExpensesToExcel(expenses, saveDialog.FileName);
                    
                    UpdateStatus($"Excel 내보내기 완료: {saveDialog.FileName}");
                    
                    MessageBox.Show($"Excel 파일로 내보내기 완료!\n\n파일: {saveDialog.FileName}", 
                        "완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel 내보내기 실패:\n{ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportExpensesExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "Excel 파일 (*.xlsx)|*.xlsx|모든 파일 (*.*)|*.*"
                };

                if (openDialog.ShowDialog() == true)
                {
                    var result = MessageBox.Show(
                        "Excel 파일의 지출 데이터를 가져옵니다.\n\n계속하시겠습니까?",
                        "확인",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var expenses = _excelService.ImportExpensesFromExcel(openDialog.FileName);
                        
                        int addedCount = 0;
                        foreach (var expense in expenses)
                        {
                            _databaseService.AddExpense(expense);
                            addedCount++;
                        }

                        RefreshExpenses();
                        UpdateStatus($"Excel 가져오기 완료: {addedCount}건 추가됨");
                        
                        MessageBox.Show($"Excel 파일에서 {addedCount}건의 지출 데이터를 가져왔습니다!", 
                            "완료", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel 가져오기 실패:\n{ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }
    }
}

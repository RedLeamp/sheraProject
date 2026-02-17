using System;
using System.Collections.Generic;
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

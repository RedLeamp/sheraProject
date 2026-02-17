using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using OfficeManagerWPF.Services;

namespace OfficeManagerWPF.Views
{
    public partial class PaymentExpenseWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly ExcelService _excelService;
        private readonly string _currentPeriod;

        public PaymentExpenseWindow()
        {
            InitializeComponent();
            SetWindowIcon();
            _dbService = new DatabaseService();
            _excelService = new ExcelService(_dbService);
            _currentPeriod = DateTime.Now.ToString("yyyy-MM");
            
            PeriodText.Text = DateTime.Now.ToString("yyyy년 MM월");
            LoadData();
        }

        private void LoadData()
        {
            // 입금 내역
            var payments = _dbService.GetPaymentsByPeriod(_currentPeriod);
            PaymentsDataGrid.ItemsSource = payments;
            TotalPaymentsText.Text = $"{payments.Sum(p => p.Amount):N0}원";

            // 지출 내역
            var expenses = _dbService.GetExpensesByPeriod(_currentPeriod);
            ExpensesDataGrid.ItemsSource = expenses;
            TotalExpensesText.Text = $"{expenses.Sum(e => e.Amount):N0}원";
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"OfficeManager_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (dialog.ShowDialog() == true)
                {
                    _excelService.ExportToExcel(dialog.FileName);
                    MessageBox.Show("Excel 파일이 성공적으로 내보내졌습니다.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"내보내기 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Excel Files|*.xlsx;*.xls"
                };

                if (dialog.ShowDialog() == true)
                {
                    _excelService.ImportFromExcel(dialog.FileName);
                    LoadData();
                    MessageBox.Show("Excel 파일을 성공적으로 가져왔습니다.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"가져오기 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    
        private void SetWindowIcon()
        {
            try
            {
                var iconUri = new Uri("pack://application:,,,/Resources/app.ico", UriKind.Absolute);
                this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"아이콘 로딩 실패: {ex.Message}");
            }
        }
    }
}

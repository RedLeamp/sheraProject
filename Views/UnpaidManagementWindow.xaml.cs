using System;
using System.Linq;
using System.Windows;
using OfficeManagerWPF.Services;

namespace OfficeManagerWPF.Views
{
    public partial class UnpaidManagementWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly string _period;

        public UnpaidManagementWindow(string period)
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _period = period;
            
            var periodDate = DateTime.Parse($"{period}-01");
            PeriodText.Text = periodDate.ToString("yyyy년 MM월");
            
            LoadUnpaidCompanies();
        }

        private void LoadUnpaidCompanies()
        {
            var allCompanies = _dbService.GetAllCompanies().Where(c => c.IsActive).ToList();
            var payments = _dbService.GetPaymentsByPeriod(_period);
            var paidCompanyIds = payments.Select(p => p.CompanyId).ToHashSet();

            var unpaidCompanies = allCompanies.Where(c => !paidCompanyIds.Contains(c.Id)).ToList();

            UnpaidCompaniesDataGrid.ItemsSource = unpaidCompanies;
            UnpaidCountText.Text = unpaidCompanies.Count.ToString();
            TotalUnpaidText.Text = $"{unpaidCompanies.Sum(c => c.MonthlyFee):N0}원";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

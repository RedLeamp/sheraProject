using System.Windows;
using OfficeManagerWPF.Services;

namespace OfficeManagerWPF.Views
{
    public partial class CompanyManagementWindow : Window
    {
        private readonly DatabaseService _dbService;

        public CompanyManagementWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            LoadCompanies();
        }

        private void LoadCompanies()
        {
            CompaniesDataGrid.ItemsSource = _dbService.GetAllCompanies();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

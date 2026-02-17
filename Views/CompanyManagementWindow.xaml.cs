using System;
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
            SetWindowIcon();
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

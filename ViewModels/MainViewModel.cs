using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using OfficeManagerWPF.Models;
using OfficeManagerWPF.Services;

namespace OfficeManagerWPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DatabaseService _dbService;
        private string _selectedPeriod;
        private decimal _totalPayments;
        private decimal _totalExpenses;
        private decimal _netProfit;
        private int _activeCompanies;
        private int _unpaidCount;

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            SelectedPeriod = DateTime.Now.ToString("yyyy-MM");
            
            OpenCompanyManagementCommand = new RelayCommand(_ => OpenCompanyManagement());
            OpenPaymentExpenseCommand = new RelayCommand(_ => OpenPaymentExpense());
            OpenUnpaidManagementCommand = new RelayCommand(_ => OpenUnpaidManagement());
            OpenDataGridCommand = new RelayCommand(_ => OpenDataGrid());
            OpenNotificationSettingsCommand = new RelayCommand(_ => OpenNotificationSettings());
            OpenNotificationLogsCommand = new RelayCommand(_ => OpenNotificationLogs());
            
            RefreshStatistics();
        }

        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (SetProperty(ref _selectedPeriod, value))
                {
                    RefreshStatistics();
                }
            }
        }

        public decimal TotalPayments
        {
            get => _totalPayments;
            set => SetProperty(ref _totalPayments, value);
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set => SetProperty(ref _totalExpenses, value);
        }

        public decimal NetProfit
        {
            get => _netProfit;
            set => SetProperty(ref _netProfit, value);
        }

        public int ActiveCompanies
        {
            get => _activeCompanies;
            set => SetProperty(ref _activeCompanies, value);
        }

        public int UnpaidCount
        {
            get => _unpaidCount;
            set => SetProperty(ref _unpaidCount, value);
        }

        public ICommand OpenCompanyManagementCommand { get; }
        public ICommand OpenPaymentExpenseCommand { get; }
        public ICommand OpenUnpaidManagementCommand { get; }
        public ICommand OpenDataGridCommand { get; }
        public ICommand OpenNotificationSettingsCommand { get; }
        public ICommand OpenNotificationLogsCommand { get; }

        private void RefreshStatistics()
        {
            // 입금/지출 통계
            var payments = _dbService.GetPaymentsByPeriod(SelectedPeriod);
            var expenses = _dbService.GetExpensesByPeriod(SelectedPeriod);
            
            TotalPayments = payments.Sum(p => p.Amount);
            TotalExpenses = expenses.Sum(e => e.Amount);
            NetProfit = TotalPayments - TotalExpenses;

            // 업체 통계
            var companies = _dbService.GetAllCompanies();
            ActiveCompanies = companies.Count(c => c.IsActive);

            // 미수금 계산
            var paidCompanyIds = payments.Select(p => p.CompanyId).ToHashSet();
            UnpaidCount = companies.Count(c => c.IsActive && !paidCompanyIds.Contains(c.Id));
        }

        private void OpenCompanyManagement()
        {
            var window = new Views.CompanyManagementWindow();
            window.ShowDialog();
            RefreshStatistics();
        }

        private void OpenPaymentExpense()
        {
            var window = new Views.PaymentExpenseWindow();
            window.ShowDialog();
            RefreshStatistics();
        }

        private void OpenUnpaidManagement()
        {
            var window = new Views.UnpaidManagementWindow(SelectedPeriod);
            window.ShowDialog();
            RefreshStatistics();
        }

        private void OpenDataGrid()
        {
            var window = new Views.DataGridWindow();
            window.ShowDialog();
            RefreshStatistics();
        }

        private void OpenNotificationSettings()
        {
            var window = new Views.NotificationSettingsWindow();
            window.ShowDialog();
        }

        private void OpenNotificationLogs()
        {
            var window = new Views.NotificationLogsWindow();
            window.ShowDialog();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using OfficeManagerWPF.Models;
using OfficeManagerWPF.Services;

namespace OfficeManagerWPF
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _dbService;

        public MainWindow()
        {
            InitializeComponent();
            
            _dbService = new DatabaseService();
            
            // 아이콘 설정 (Code-Behind 방식)
            SetWindowIcon();
            
            // 지역 목록 로드
            LoadLocations();
            
            // 대시보드 지역별 탭 생성
            LoadDashboardLocationTabs();
        }

        private void SetWindowIcon()
        {
            try
            {
                // WPF 리소스에서 아이콘 로드
                var iconUri = new Uri("pack://application:,,,/Resources/app.ico", UriKind.Absolute);
                this.Icon = BitmapFrame.Create(iconUri);
            }
            catch (Exception ex)
            {
                // 아이콘 로딩 실패 시 무시 (프로그램은 정상 작동)
                System.Diagnostics.Debug.WriteLine($"아이콘 로딩 실패: {ex.Message}");
            }
        }

        // 지역 추가
        private void AddLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string locationName = LocationNameInput.Text.Trim();
                
                if (string.IsNullOrEmpty(locationName))
                {
                    MessageBox.Show("지역명을 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var location = new Location
                {
                    Name = locationName,
                    Address = LocationAddressInput.Text.Trim(),
                    Manager = LocationManagerInput.Text.Trim(),
                    PhoneNumber = LocationPhoneInput.Text.Trim(),
                    Notes = LocationNotesInput.Text.Trim(),
                    IsActive = true
                };

                _dbService.AddLocation(location);
                
                MessageBox.Show($"'{locationName}' 지역이 추가되었습니다.", "추가 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // 입력 폼 초기화
                LocationNameInput.Clear();
                LocationAddressInput.Clear();
                LocationManagerInput.Clear();
                LocationPhoneInput.Clear();
                LocationNotesInput.Clear();
                
                // 목록 새로고침
                LoadLocations();
                LoadDashboardLocationTabs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"지역 추가 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 지역 목록 로드
        private void RefreshLocations_Click(object sender, RoutedEventArgs e)
        {
            LoadLocations();
        }

        private void LoadLocations()
        {
            try
            {
                var locations = _dbService.GetAllLocations();
                LocationsListBox.ItemsSource = locations;
                
                // 상태 업데이트
                LocationStatusText.Text = locations.Count > 0 
                    ? $"등록된 지역: {locations.Count}개" 
                    : "등록된 지역이 없습니다";
            }
            catch (Exception ex)
            {
                LocationStatusText.Text = $"오류: {ex.Message}";
            }
        }

        // 대시보드 지역별 탭 생성
        private void LoadDashboardLocationTabs()
        {
            try
            {
                var locations = _dbService.GetAllLocations();
                
                if (locations == null || locations.Count == 0)
                {
                    // 지역이 없으면 표시 안 함
                    LocationFilterContainer.Visibility = Visibility.Collapsed;
                    return;
                }

                LocationFilterContainer.Visibility = Visibility.Visible;
                LocationFilterTabs.Items.Clear();

                // "전체" 탭 추가
                var allTab = new TabItem
                {
                    Header = "📍 전체",
                    Tag = ""
                };
                LocationFilterTabs.Items.Add(allTab);

                // 각 지역별 탭 추가
                foreach (var location in locations.Where(l => l.IsActive))
                {
                    var tab = new TabItem
                    {
                        Header = $"📍 {location.Name}",
                        Tag = location.Name
                    };
                    LocationFilterTabs.Items.Add(tab);
                }

                LocationFilterTabs.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"지역 탭 로드 실패: {ex.Message}");
            }
        }

        // 데이터 초기화 (업체+입금)
        private void ClearCompanyData_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "업체 데이터를 모두 삭제하시겠습니까?\n(입금 내역도 함께 삭제됩니다)",
                "데이터 초기화 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _dbService.ClearAllCompanies();
                    _dbService.ClearAllPayments();
                    MessageBox.Show("업체 및 입금 데이터가 초기화되었습니다.", "초기화 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"초기화 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 데이터 초기화 (지출)
        private void ClearExpenseData_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "지출 데이터를 모두 삭제하시겠습니까?",
                "데이터 초기화 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _dbService.ClearAllExpenses();
                    MessageBox.Show("지출 데이터가 초기화되었습니다.", "초기화 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"초기화 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 지역 초기화
        private void ClearLocationData_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "지역 데이터를 모두 삭제하시겠습니까?",
                "데이터 초기화 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _dbService.ClearAllLocations();
                    MessageBox.Show("지역 데이터가 초기화되었습니다.", "초기화 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadLocations();
                    LoadDashboardLocationTabs();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"초기화 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

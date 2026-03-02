using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using OfficeManagerWPF.Models;
using OfficeManagerWPF.Services;

namespace OfficeManagerWPF
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly ExcelService _excelService;

        public MainWindow()
        {
            InitializeComponent();
            
            _dbService = new DatabaseService();
            _excelService = new ExcelService();
            
            // 아이콘 설정 (Code-Behind 방식)
            SetWindowIcon();
            
            // 지역 목록 로드
            LoadLocations();
            
            // 업체 데이터 로드
            LoadCompanyData();
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"초기화 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 업체 데이터 로드
        private void LoadCompanyData()
        {
            try
            {
                var companies = _dbService.GetAllCompanies();
                
                // 전체 탭의 DataGrid
                CompaniesDataGrid.ItemsSource = companies;
                CompanyStatusText.Text = $"상태: {companies.Count}개 업체 로드됨 (전체)";
                
                // 남양 탭의 DataGrid
                var namyangCompanies = companies.Where(c => c.Location == "남양").ToList();
                NamyangCompanyDataGrid.ItemsSource = namyangCompanies;
                NamyangCompanyStatusText.Text = $"상태: {namyangCompanies.Count}개 업체 로드됨 (남양)";
                
                // 향남 탭의 DataGrid
                var hyangnamCompanies = companies.Where(c => c.Location == "향남").ToList();
                HyangnamCompanyDataGrid.ItemsSource = hyangnamCompanies;
                HyangnamCompanyStatusText.Text = $"상태: {hyangnamCompanies.Count}개 업체 로드됨 (향남)";
            }
            catch (Exception ex)
            {
                CompanyStatusText.Text = $"오류: {ex.Message}";
                if (NamyangCompanyStatusText != null)
                    NamyangCompanyStatusText.Text = $"오류: {ex.Message}";
                if (HyangnamCompanyStatusText != null)
                    HyangnamCompanyStatusText.Text = $"오류: {ex.Message}";
            }
        }

        // 업체 데이터 새로고침
        private void RefreshCompanyData_Click(object sender, RoutedEventArgs e)
        {
            LoadCompanyData();
        }

        // 임대내역 가져오기
        private void ImportRentalRecords_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Excel 파일 (*.xlsx)|*.xlsx|모든 파일 (*.*)|*.*",
                    Title = "임대내역 엑셀 파일 선택",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

                    // 파일명에서 지역 추출 (예: "2026_남양임대내역" -> "남양")
                    string locationPrefix = "";
                    if (fileName.Contains("남양"))
                        locationPrefix = "남양";
                    else if (fileName.Contains("향남"))
                        locationPrefix = "향남";

                    // 사용자에게 확인
                    var confirmResult = MessageBox.Show(
                        $"파일: {fileName}\n" +
                        $"감지된 지역: {(string.IsNullOrEmpty(locationPrefix) ? "없음" : locationPrefix)}\n\n" +
                        $"업체 데이터와 입금 데이터를 가져오시겠습니까?",
                        "임대내역 가져오기 확인",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirmResult != MessageBoxResult.Yes)
                        return;

                    // 엑셀 파일에서 데이터 읽기
                    var (companies, payments) = _excelService.ImportComplexExcel(filePath, locationPrefix);

                    // 업체 ID 매핑용 딕셔너리
                    var companyIdMap = new Dictionary<string, int>();
                    int addedCompanies = 0;
                    int addedPayments = 0;
                    int skippedPayments = 0;

                    // 1단계: 업체 추가 및 ID 매핑
                    foreach (var company in companies)
                    {
                        try
                        {
                            _dbService.AddCompany(company);
                            
                            // 추가된 업체 조회 (ID 얻기)
                            var addedCompany = _dbService.GetAllCompanies()
                                .FirstOrDefault(c => c.Name == company.Name && c.PhoneNumber == company.PhoneNumber);
                            
                            if (addedCompany != null)
                            {
                                // 지역 접두사 제거한 이름으로 매핑
                                string cleanName = company.Name.Replace($"[{locationPrefix}] ", "");
                                companyIdMap[cleanName] = addedCompany.Id;
                                addedCompanies++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"업체 추가 실패: {company.Name} - {ex.Message}");
                        }
                    }

                    // 2단계: 입금 데이터 추가 (CompanyId 매핑)
                    foreach (var payment in payments)
                    {
                        try
                        {
                            // CompanyId 매핑
                            if (companyIdMap.ContainsKey(payment.CompanyName))
                            {
                                payment.CompanyId = companyIdMap[payment.CompanyName];
                                _dbService.AddPayment(payment);
                                addedPayments++;
                            }
                            else
                            {
                                // 매핑 실패 시 부분 이름 검색
                                var existingCompany = _dbService.GetAllCompanies()
                                    .FirstOrDefault(c => c.Name.Contains(payment.CompanyName));
                                
                                if (existingCompany != null)
                                {
                                    payment.CompanyId = existingCompany.Id;
                                    _dbService.AddPayment(payment);
                                    addedPayments++;
                                }
                                else
                                {
                                    skippedPayments++;
                                    System.Diagnostics.Debug.WriteLine($"입금 데이터 건너뜀: {payment.CompanyName} (업체를 찾을 수 없음)");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            skippedPayments++;
                            System.Diagnostics.Debug.WriteLine($"입금 추가 실패: {payment.CompanyName} - {ex.Message}");
                        }
                    }

                    // 결과 표시
                    string resultMessage = $"임대내역 가져오기 완료!\n\n" +
                                         $"추가된 업체: {addedCompanies}개\n" +
                                         $"추가된 입금: {addedPayments}개\n" +
                                         $"건너뛴 입금: {skippedPayments}개\n" +
                                         $"총 처리: {companies.Count + payments.Count}개";

                    MessageBox.Show(resultMessage, "가져오기 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // 데이터 새로고침
                    LoadCompanyData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"임대내역 가져오기 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

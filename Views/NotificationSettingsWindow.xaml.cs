using System;
using System.Windows;
using OfficeManagerWPF.Services;

namespace OfficeManagerWPF.Views
{
    public partial class NotificationSettingsWindow : Window
    {
        private readonly SmsService _smsService;
        private readonly EmailService _emailService;
        private readonly DatabaseService _databaseService;
        private readonly NotificationSchedulerService _schedulerService;

        public NotificationSettingsWindow()
        {
            InitializeComponent();
            _smsService = new SmsService();
            _emailService = new EmailService();
            _databaseService = new DatabaseService();
            _schedulerService = new NotificationSchedulerService();

            LoadSettings();
        }

        private void LoadSettings()
        {
            // SMS 설정 로드
            txtSmsApiKey.Text = Properties.Settings.Default.SmsApiKey ?? "";
            txtSmsSenderNumber.Text = Properties.Settings.Default.SmsSenderNumber ?? "";

            // 이메일 설정 로드
            txtSmtpServer.Text = Properties.Settings.Default.SmtpServer ?? "smtp.gmail.com";
            txtSmtpPort.Text = Properties.Settings.Default.SmtpPort > 0 ? 
                Properties.Settings.Default.SmtpPort.ToString() : "587";
            txtEmailAddress.Text = Properties.Settings.Default.EmailAddress ?? "";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // SMS 설정 저장
                _smsService.UpdateSettings(
                    txtSmsApiKey.Text,
                    txtSmsApiSecret.Password,
                    txtSmsSenderNumber.Text
                );

                // 이메일 설정 저장
                if (int.TryParse(txtSmtpPort.Text, out int port))
                {
                    _emailService.UpdateSettings(
                        txtSmtpServer.Text,
                        port,
                        txtEmailAddress.Text,
                        txtEmailPassword.Password
                    );

                    Properties.Settings.Default.SmtpServer = txtSmtpServer.Text;
                    Properties.Settings.Default.SmtpPort = port;
                    Properties.Settings.Default.Save();
                }

                MessageBox.Show(
                    "알림 설정이 저장되었습니다.\n자동 알림이 활성화됩니다.",
                    "설정 저장",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"설정 저장 중 오류가 발생했습니다:\n{ex.Message}",
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void TestNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 테스트용 첫 번째 업체 가져오기
                var companies = _databaseService.GetAllCompanies();
                if (companies.Count == 0)
                {
                    MessageBox.Show(
                        "테스트할 업체가 없습니다.\n먼저 업체를 등록해주세요.",
                        "알림",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    return;
                }

                var testCompany = companies[0];
                
                // 임시로 설정 적용
                _smsService.UpdateSettings(
                    txtSmsApiKey.Text,
                    txtSmsApiSecret.Password,
                    txtSmsSenderNumber.Text
                );

                if (int.TryParse(txtSmtpPort.Text, out int port))
                {
                    _emailService.UpdateSettings(
                        txtSmtpServer.Text,
                        port,
                        txtEmailAddress.Text,
                        txtEmailPassword.Password
                    );
                }

                // 테스트 알림 발송
                var result = await _schedulerService.SendTestNotificationAsync(testCompany);

                if (result)
                {
                    MessageBox.Show(
                        $"테스트 알림이 발송되었습니다.\n\n업체: {testCompany.Name}\n연락처: {testCompany.PhoneNumber}\n이메일: {testCompany.Email}",
                        "테스트 발송 완료",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "테스트 알림 발송에 실패했습니다.\nAPI 설정을 확인해주세요.",
                        "발송 실패",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"테스트 발송 중 오류가 발생했습니다:\n{ex.Message}",
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

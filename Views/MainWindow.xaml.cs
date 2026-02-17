using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OfficeManagerWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // 아이콘 설정 (Code-Behind 방식)
            SetWindowIcon();
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
    }
}

using System;
using System.Collections.Generic;
using System.Windows;

namespace OfficeManagerWPF.Views
{
    public partial class NotificationLogsWindow : Window
    {
        public NotificationLogsWindow()
        {
            InitializeComponent();
            SetWindowIcon();
            LoadNotificationLogs();
        }

        private void LoadNotificationLogs()
        {
            // 시뮬레이션 데이터 (실제로는 데이터베이스에서 조회)
            var logs = new List<NotificationLog>
            {
                new NotificationLog 
                { 
                    SentDate = DateTime.Now.AddDays(-1), 
                    CompanyName = "테크스타트업", 
                    Type = "월세안내", 
                    Method = "SMS+이메일", 
                    Message = "월세 납입일 7일 전 안내", 
                    Status = "성공" 
                },
                new NotificationLog 
                { 
                    SentDate = DateTime.Now.AddDays(-5), 
                    CompanyName = "디자인스튜디오", 
                    Type = "미수금", 
                    Method = "SMS", 
                    Message = "2024-01 미수금 납입 안내", 
                    Status = "성공" 
                },
                new NotificationLog 
                { 
                    SentDate = DateTime.Now.AddDays(-15), 
                    CompanyName = "마케팅에이전시", 
                    Type = "월세안내", 
                    Method = "이메일", 
                    Message = "월세 납입일 당일 안내", 
                    Status = "실패" 
                }
            };

            dgNotificationLogs.ItemsSource = logs;
        }

        public class NotificationLog
        {
            public DateTime SentDate { get; set; }
            public string CompanyName { get; set; } = "";
            public string Type { get; set; } = "";
            public string Method { get; set; } = "";
            public string Message { get; set; } = "";
            public string Status { get; set; } = "";
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

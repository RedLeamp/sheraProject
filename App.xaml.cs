using System.Windows;
using OfficeManagerWPF.Services;

namespace OfficeManagerWPF
{
    public partial class App : Application
    {
        private NotificationSchedulerService? _schedulerService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 자동 알림 스케줄러 시작 (항상 활성화)
            _schedulerService = new NotificationSchedulerService();
            _schedulerService.Start();
            System.Diagnostics.Debug.WriteLine("📧 자동 알림 스케줄러가 시작되었습니다.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 스케줄러 중지
            _schedulerService?.Stop();
            base.OnExit(e);
        }
    }
}

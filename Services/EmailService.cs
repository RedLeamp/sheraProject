using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OfficeManagerWPF.Services
{
    /// <summary>
    /// 이메일 발송 서비스
    /// SMTP를 사용하여 이메일 발송
    /// </summary>
    public class EmailService
    {
        private string smtpServer;
        private int smtpPort;
        private string senderEmail;
        private string senderPassword;

        public EmailService()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            // 설정 파일에서 로드 (예시: Gmail SMTP)
            smtpServer = "smtp.gmail.com";
            smtpPort = 587;
            senderEmail = Properties.Settings.Default.EmailAddress ?? "";
            senderPassword = Properties.Settings.Default.EmailPassword ?? "";
        }

        /// <summary>
        /// SMTP 설정 업데이트
        /// </summary>
        public void UpdateSettings(string server, int port, string email, string password)
        {
            smtpServer = server;
            smtpPort = port;
            senderEmail = email;
            senderPassword = password;

            Properties.Settings.Default.EmailAddress = email;
            Properties.Settings.Default.EmailPassword = password;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 이메일 발송
        /// </summary>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    throw new InvalidOperationException("이메일 설정이 완료되지 않았습니다.");
                }

                using (var message = new MailMessage(senderEmail, toEmail))
                {
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = false;

                    using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

                        await smtpClient.SendMailAsync(message);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"이메일 발송 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 월세 납입 안내 이메일 발송
        /// </summary>
        public async Task<bool> SendRentReminderEmailAsync(string toEmail, string companyName, decimal amount, int daysRemaining)
        {
            string subject = daysRemaining switch
            {
                7 => $"[월세 안내] {companyName} - 월세 납입 안내 (D-7)",
                3 => $"[월세 안내] {companyName} - 월세 납입 리마인드 (D-3)",
                0 => $"[월세 안내] {companyName} - 오늘 월세 납입일입니다",
                _ => $"[월세 안내] {companyName}"
            };

            string body = $@"안녕하세요, {companyName} 담당자님.

월세 납입 안내 드립니다.

📅 납입 예정일: {DateTime.Now.ToString("yyyy년 MM월 dd일")}
💰 납입 금액: {amount:N0}원
⏰ 남은 기간: {daysRemaining}일

정해진 기일 내에 입금 부탁드립니다.

감사합니다.
오피스 매니저 드림";

            return await SendEmailAsync(toEmail, subject, body);
        }

        /// <summary>
        /// 미수금 안내 이메일 발송
        /// </summary>
        public async Task<bool> SendUnpaidReminderEmailAsync(string toEmail, string companyName, decimal amount, string period)
        {
            string subject = $"[미수금 안내] {companyName} - {period} 미수금 납입 안내";

            string body = $@"안녕하세요, {companyName} 담당자님.

미수금 납입 안내 드립니다.

📅 대상 기간: {period}
💰 미수금 금액: {amount:N0}원
⚠️ 상태: 미납

빠른 시일 내에 입금 부탁드립니다.

감사합니다.
오피스 매니저 드림";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}

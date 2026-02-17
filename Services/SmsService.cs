using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OfficeManagerWPF.Services
{
    /// <summary>
    /// SMS 발송 서비스
    /// 실제 SMS API 연동 (알리고, 카카오 알림톡, 네이버 클라우드 등)
    /// </summary>
    public class SmsService
    {
        private string apiKey;
        private string apiSecret;
        private string senderNumber;
        private readonly HttpClient _httpClient;

        public SmsService()
        {
            _httpClient = new HttpClient();
            LoadSettings();
        }

        private void LoadSettings()
        {
            apiKey = Properties.Settings.Default.SmsApiKey ?? "";
            apiSecret = Properties.Settings.Default.SmsApiSecret ?? "";
            senderNumber = Properties.Settings.Default.SmsSenderNumber ?? "";
        }

        /// <summary>
        /// SMS API 설정 업데이트
        /// </summary>
        public void UpdateSettings(string key, string secret, string sender)
        {
            apiKey = key;
            apiSecret = secret;
            senderNumber = sender;

            Properties.Settings.Default.SmsApiKey = key;
            Properties.Settings.Default.SmsApiSecret = secret;
            Properties.Settings.Default.SmsSenderNumber = sender;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// SMS 발송 (실제 API 연동)
        /// 알리고 SMS API 기준 예제
        /// </summary>
        public async Task<bool> SendSmsAsync(string toPhoneNumber, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(senderNumber))
                {
                    throw new InvalidOperationException("SMS 설정이 완료되지 않았습니다.");
                }

                // 실제 SMS API 연동 예제 (알리고 SMS)
                /*
                var formData = new Dictionary<string, string>
                {
                    { "key", apiKey },
                    { "user_id", apiSecret },
                    { "sender", senderNumber },
                    { "receiver", toPhoneNumber },
                    { "msg", message },
                    { "msg_type", "SMS" }
                };

                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync("https://apis.aligo.in/send/", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    // 응답 파싱 및 성공 여부 확인
                    return true;
                }
                return false;
                */

                // 시뮬레이션 모드 (실제 API 연동 시 위 코드 활성화)
                await Task.Delay(100);
                System.Diagnostics.Debug.WriteLine($"[SMS 발송 시뮬레이션] {toPhoneNumber}: {message}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SMS 발송 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 월세 납입 안내 SMS 발송
        /// </summary>
        public async Task<bool> SendRentReminderSmsAsync(string toPhoneNumber, string companyName, decimal amount, int daysRemaining)
        {
            string message = daysRemaining switch
            {
                7 => $"[월세 안내]\n{companyName}님\n월세 납입일이 7일 남았습니다.\n금액: {amount:N0}원\n기한 내 납부 부탁드립니다.",
                3 => $"[월세 안내]\n{companyName}님\n월세 납입일이 3일 남았습니다.\n금액: {amount:N0}원\n기한 내 납부 부탁드립니다.",
                0 => $"[월세 안내]\n{companyName}님\n오늘은 월세 납입일입니다.\n금액: {amount:N0}원\n납부 부탁드립니다.",
                _ => $"[월세 안내]\n{companyName}님\n월세 {amount:N0}원 납부 안내"
            };

            return await SendSmsAsync(toPhoneNumber, message);
        }

        /// <summary>
        /// 미수금 안내 SMS 발송
        /// </summary>
        public async Task<bool> SendUnpaidReminderSmsAsync(string toPhoneNumber, string companyName, decimal amount, string period)
        {
            string message = $"[미수금 안내]\n{companyName}님\n{period} 미수금: {amount:N0}원\n빠른 납부 부탁드립니다.";
            return await SendSmsAsync(toPhoneNumber, message);
        }
    }
}

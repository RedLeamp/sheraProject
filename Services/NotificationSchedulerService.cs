using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using OfficeManagerWPF.Models;

namespace OfficeManagerWPF.Services
{
    /// <summary>
    /// 자동 알림 스케줄러 서비스
    /// - 월초/중순/월말 미수금 안내 (1일, 15일, 말일)
    /// - 월세 납입 안내 (7일전, 3일전, 당일)
    /// </summary>
    public class NotificationSchedulerService
    {
        private readonly DatabaseService _databaseService;
        private readonly SmsService _smsService;
        private readonly EmailService _emailService;
        private Timer? _timer;
        private bool _isRunning;

        public NotificationSchedulerService()
        {
            _databaseService = new DatabaseService();
            _smsService = new SmsService();
            _emailService = new EmailService();
        }

        /// <summary>
        /// 스케줄러 시작 (매일 오전 9시 체크)
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            
            // 즉시 한 번 실행
            Task.Run(() => CheckAndSendNotifications());

            // 매 1시간마다 체크 (실제 운영에서는 매일 특정 시간으로 설정)
            _timer = new Timer(
                async _ => await CheckAndSendNotifications(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromHours(1)
            );

            System.Diagnostics.Debug.WriteLine("알림 스케줄러 시작됨");
        }

        /// <summary>
        /// 스케줄러 중지
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _timer?.Dispose();
            _timer = null;
            System.Diagnostics.Debug.WriteLine("알림 스케줄러 중지됨");
        }

        /// <summary>
        /// 알림 체크 및 발송 메인 로직
        /// </summary>
        private async Task CheckAndSendNotifications()
        {
            try
            {
                var today = DateTime.Today;
                var currentDay = today.Day;
                var lastDayOfMonth = DateTime.DaysInMonth(today.Year, today.Month);

                // 1️⃣ 미수금 안내 (월초/중순/월말)
                if (currentDay == 1 || currentDay == 15 || currentDay == lastDayOfMonth)
                {
                    await SendUnpaidReminders(today);
                }

                // 2️⃣ 월세 납입 안내 (7일전, 3일전, 당일)
                await SendRentReminders(today);

                System.Diagnostics.Debug.WriteLine($"알림 체크 완료: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"알림 체크 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 미수금 안내 발송 (월초/중순/월말)
        /// </summary>
        private async Task SendUnpaidReminders(DateTime today)
        {
            try
            {
                var companies = _databaseService.GetAllCompanies();
                var currentPeriod = today.ToString("yyyy-MM");

                foreach (var company in companies.Where(c => c.IsActive))
                {
                    // 미수금 계산
                    var payments = _databaseService.GetPaymentsByCompanyAndPeriod(company.Id, currentPeriod);
                    var totalPaid = payments.Sum(p => p.Amount);
                    var unpaidAmount = company.MonthlyFee - totalPaid;

                    if (unpaidAmount > 0)
                    {
                        // SMS 발송
                        if (!string.IsNullOrEmpty(company.PhoneNumber))
                        {
                            await _smsService.SendUnpaidReminderSmsAsync(
                                company.PhoneNumber,
                                company.Name,
                                unpaidAmount,
                                currentPeriod
                            );
                        }

                        // 이메일 발송
                        if (!string.IsNullOrEmpty(company.Email))
                        {
                            await _emailService.SendUnpaidReminderEmailAsync(
                                company.Email,
                                company.Name,
                                unpaidAmount,
                                currentPeriod
                            );
                        }

                        System.Diagnostics.Debug.WriteLine($"미수금 안내 발송: {company.Name} - {unpaidAmount:N0}원");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"미수금 안내 발송 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 월세 납입 안내 발송 (7일전, 3일전, 당일)
        /// </summary>
        private async Task SendRentReminders(DateTime today)
        {
            try
            {
                var companies = _databaseService.GetAllCompanies();

                foreach (var company in companies.Where(c => c.IsActive))
                {
                    // 매월 계약일자를 납입일로 설정
                    var contractDay = company.ContractDate.Day;
                    var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
                    var paymentDay = Math.Min(contractDay, daysInMonth);

                    var paymentDate = new DateTime(today.Year, today.Month, paymentDay);
                    var daysRemaining = (paymentDate - today).Days;

                    // 7일전, 3일전, 당일에만 발송
                    if (daysRemaining == 7 || daysRemaining == 3 || daysRemaining == 0)
                    {
                        // SMS 발송
                        if (!string.IsNullOrEmpty(company.PhoneNumber))
                        {
                            await _smsService.SendRentReminderSmsAsync(
                                company.PhoneNumber,
                                company.Name,
                                company.MonthlyFee,
                                daysRemaining
                            );
                        }

                        // 이메일 발송
                        if (!string.IsNullOrEmpty(company.Email))
                        {
                            await _emailService.SendRentReminderEmailAsync(
                                company.Email,
                                company.Name,
                                company.MonthlyFee,
                                daysRemaining
                            );
                        }

                        System.Diagnostics.Debug.WriteLine($"월세 안내 발송: {company.Name} - D-{daysRemaining}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"월세 안내 발송 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 수동 테스트 발송
        /// </summary>
        public async Task<bool> SendTestNotificationAsync(Company company)
        {
            try
            {
                var result = true;

                // SMS 테스트
                if (!string.IsNullOrEmpty(company.PhoneNumber))
                {
                    result &= await _smsService.SendRentReminderSmsAsync(
                        company.PhoneNumber,
                        company.Name,
                        company.MonthlyFee,
                        0
                    );
                }

                // 이메일 테스트
                if (!string.IsNullOrEmpty(company.Email))
                {
                    result &= await _emailService.SendRentReminderEmailAsync(
                        company.Email,
                        company.Name,
                        company.MonthlyFee,
                        0
                    );
                }

                return result;
            }
            catch
            {
                return false;
            }
        }
    }
}

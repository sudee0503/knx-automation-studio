using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClosedXML.Excel;
using Microsoft.Win32;
using Sude.Models;
using Sude.Services;
using Sude.Views;
using System;
using System.Threading.Tasks;

namespace Sude.ViewModels.AdminViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;

        [ObservableProperty] private string _newUsername;
        [ObservableProperty] private string _newPassword;
        [ObservableProperty] private string _confirmPassword;

        public SettingViewModel()
        {
            _dbService = new DatabaseService();
        }

        [RelayCommand]
        private async Task UpdateAdminInfo()
        {
            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                CustomMessageBoxWindow.Show("Lütfen tüm alanları doldurun!", "Uyarı", CustomMessageBoxType.Warning);
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                CustomMessageBoxWindow.Show("Girdiğiniz şifreler birbiriyle uyuşmuyor!", "Hata", CustomMessageBoxType.Error);
                return;
            }

            try
            {
                int adminId = SessionService.ActiveUserId;
                await _dbService.UpdateAdminCredentialsAsync(adminId, NewUsername, NewPassword);

                await _dbService.LogActionAsync(adminId, IslemTuru.KullaniciGuncellendi, targetUserId: adminId);

                CustomMessageBoxWindow.Show("Yönetici giriş bilgileriniz başarıyla güncellendi! Lütfen yeni bilgilerinizi unutmayın.", "Başarılı", CustomMessageBoxType.Success);

                NewUsername = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"Güncelleme sırasında bir hata oluştu:\n{ex.Message}", "Hata", CustomMessageBoxType.Error);
            }
        }
        [RelayCommand]
        private async Task ArchiveAndCleanLogs()
        {
            bool confirm = CustomMessageBoxWindow.Show("1 yıldan eski tüm işlem kayıtları Excel dosyasına aktarılacak ve veritabanından KALICI OLARAK SİLİNECEKTİR.\n\nİşlemi onaylıyor musunuz?", "Arşivleme Onayı", CustomMessageBoxType.Warning, CustomMessageBoxButtons.YesNo);

            if (!confirm) return;

            DateTime thresholdDate = DateTime.Now.AddYears(-1);

            try
            {
                var oldLogs = await _dbService.GetLogsOlderThanAsync(thresholdDate);

                if (oldLogs == null || oldLogs.Count == 0)
                {
                    CustomMessageBoxWindow.Show("Veritabanında 1 yıldan eski, arşivlenecek kayıt bulunmuyor.", "Bilgi", CustomMessageBoxType.Info);
                    return;
                }

                var sfd = new SaveFileDialog
                {
                    Filter = "Excel Dosyası|*.xlsx",
                    Title = "Eski Logları Arşivle",
                    FileName = $"KNX_Log_Arsivi_{thresholdDate:yyyy}_ve_oncesi.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Eski Kayıtlar");

                        worksheet.Cell(1, 1).Value = "İşlemi Yapan";
                        worksheet.Cell(1, 2).Value = "İşlem Detayı";
                        worksheet.Cell(1, 3).Value = "İşlem Tarihi";
                        worksheet.Range("A1:C1").Style.Font.Bold = true;
                        worksheet.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.LightGray;

                        for (int i = 0; i < oldLogs.Count; i++)
                        {
                            var log = oldLogs[i];
                            worksheet.Cell(i + 2, 1).Value = log.KullaniciAdi;
                            worksheet.Cell(i + 2, 2).Value = log.IslemAciklamasi;
                            worksheet.Cell(i + 2, 3).Value = log.Tarihi.ToString("dd.MM.yyyy HH:mm:ss");
                        }
                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }
                    await _dbService.DeleteLogsOlderThanAsync(thresholdDate);

                    CustomMessageBoxWindow.Show($"{oldLogs.Count} adet eski kayıt başarıyla Excel'e arşivlendi ve veritabanı temizlendi!", "Arşivleme Başarılı", CustomMessageBoxType.Success);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"Arşivleme sırasında hata oluştu. Veriler SİLİNMEDİ!\nHata: {ex.Message}", "Hata", CustomMessageBoxType.Error);
            }
        }
    }
}
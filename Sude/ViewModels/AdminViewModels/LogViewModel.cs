using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClosedXML.Excel;
using Microsoft.Win32;
using Sude.Models;
using Sude.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Sude.Views;
using Sude.Views.AdminViews;

namespace Sude.ViewModels.AdminViewModels
{
    public partial class LogViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;

        [ObservableProperty]
        private ObservableCollection<Log> _logs = new();

        [ObservableProperty]
        private Log _currentFilter = new Log();

        public LogViewModel()
        {
            _dbService = new DatabaseService();
            _ = LoadLogsAsync();
        }

        private async Task LoadLogsAsync()
        {
            var logList = await _dbService.GetAllLogsAsync(CurrentFilter);
            Logs.Clear();
            foreach (var log in logList)
            {
                Logs.Add(log);
            }
        }

        [RelayCommand]
        private void OpenFilter()
        {
            var filterVM = new DialogWindowViewModels.LogFilterViewModel(CurrentFilter);
            var filterView = new Views.AdminViews.DialogViews.LogFilter
            {
                DataContext = filterVM
            };
            var dialogWindow = new DialogWindow(filterView)
            {
                Content = filterView,
                Title = "Kayıtları Filtrele",
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
            };
            filterVM.CloseAction = () => dialogWindow.Close();
            dialogWindow.ShowDialog();
            if (filterVM.FilterLog != null)
            {
                CurrentFilter = filterVM.FilterLog;
                _ = LoadLogsAsync();
            }
        }
        [RelayCommand]
        private void ExportToExcel()
        {
            if (Logs == null || Logs.Count == 0)
            {
                CustomMessageBoxWindow.Show("Dışa aktarılacak herhangi bir kayıt bulunamadı!", "Uyarı", CustomMessageBoxType.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Excel Dosyası|*.xlsx",
                Title = "Logları Excel'e Aktar",
                FileName = $"Islem_Kayitlari_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("İşlem Kayıtları");


                        worksheet.Cell(1, 1).Value = "İşlemi Yapan";
                        worksheet.Cell(1, 2).Value = "İşlem Detayı";
                        worksheet.Cell(1, 3).Value = "İşlem Tarihi";
                        worksheet.Range("A1:C1").Style.Font.Bold = true;
                        worksheet.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.LightGray;

                        for (int i = 0; i < Logs.Count; i++)
                        {
                            var log = Logs[i];
                            worksheet.Cell(i + 2, 1).Value = log.KullaniciAdi;
                            worksheet.Cell(i + 2, 2).Value = log.IslemAciklamasi;
                            worksheet.Cell(i + 2, 3).Value = log.Tarihi.ToString("dd.MM.yyyy HH:mm:ss");
                        }

                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(sfd.FileName);
                    }

                    CustomMessageBoxWindow.Show("Veriler başarıyla Excel'e aktarıldı!", "Başarılı", CustomMessageBoxType.Success);
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show($"Excel oluşturulurken bir hata meydana geldi. Dosya açık kalmış olabilir:\n{ex.Message}", "Hata", CustomMessageBoxType.Error);
                }
            }
        }

        [RelayCommand]
        private async Task ClearMainFilter()
        {
            CurrentFilter = new Log();
            await LoadLogsAsync();
        }
    }
}
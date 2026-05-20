using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sude.Models;
using Sude.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Sude.ViewModels.AdminViewModels.DialogWindowViewModels
{
    public partial class LogFilterViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        public Action CloseAction { get; set; }
        public Func<object, Task> OnFilterApplied { get; internal set; }
        public Action OnClose { get; internal set; }

        [ObservableProperty]
        private Log _filterLog;

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        [ObservableProperty]
        private ObservableCollection<dynamic> _islemTurleri = new();

        public LogFilterViewModel(Log mevcutFiltre = null)
        {
            _dbService = new DatabaseService();

            FilterLog = mevcutFiltre ?? new Log();

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var userList = await _dbService.GetUsersAsync();
                foreach (var user in userList)
                {
                    Users.Add(user);
                }

                var turler = await _dbService.GetIslemTurleriTableAsync();
                foreach (var tur in turler)
                {
                    IslemTurleri.Add(tur);
                }
            }
            catch (Exception ex)
            {
                Views.CustomMessageBoxWindow.Show("Filtre verileri yüklenirken hata oluştu:\n" + ex.Message, "Hata", Views.CustomMessageBoxType.Error);
            }
        }

        [RelayCommand]
        private void QuickFilter(string param)
        {
            if (param == "Today")
            {
                FilterLog.FiltreBaslangic = DateTime.Today;
                FilterLog.FiltreBitis = DateTime.Today.AddDays(1).AddTicks(-1);
            }
            else if (param == "LastWeek")
            {
                FilterLog.FiltreBaslangic = DateTime.Today.AddDays(-7);
                FilterLog.FiltreBitis = DateTime.Today.AddDays(1).AddTicks(-1);
            }

            OnPropertyChanged(nameof(FilterLog));
        }

        [RelayCommand]
        private void ClearFilter()
        {
            FilterLog = new Log();
        }

        [RelayCommand]
        private void Save()
        {
            CloseAction?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            FilterLog = null;
            CloseAction?.Invoke();
        }
    }
}
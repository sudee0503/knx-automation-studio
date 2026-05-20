using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Sude.Models;
using Sude.Services;
using Sude.Views.AdminViews;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Sude.ViewModels.AdminViewModels
{
    public partial class AdminPanelViewModel : ObservableObject
    {

        private readonly MainViewModel _mainRouter;

        public AdminPanelViewModel(MainViewModel mainRouter = null)
        {
            _mainRouter = mainRouter;
            CurrentView = new UC_Dashboard();
            _ = SetupDashboardStatsAsync();
        }

        [ObservableProperty]
        private string _selectedMenuItem = "Anasayfa";

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty] 
        private string _statBox1;
        
        [ObservableProperty] 
        private string _statBox2;
        
        [ObservableProperty] 
        private string _statBox3;
        
        [ObservableProperty] 
        private string _statBox4;
        
        [ObservableProperty] 
        private SeriesCollection _weeklySeries;
        
        [ObservableProperty] 
        private List<string> _weeklyLabels;
        
        [ObservableProperty] 
        private Func<double, string> _yFormatter;
        
        [ObservableProperty] 
        private ObservableCollection<Log> _recentLogs = new();

        [ObservableProperty] 
        private string _chartTitle = "Son 7 Günlük İşlem Analizi";

        public AdminPanelViewModel()
        {
            CurrentView = new UC_Dashboard();
            _ = SetupDashboardStatsAsync();
        }
        [RelayCommand]
        private void OpenDashboard()
        {
            SelectedMenuItem = "Anasayfa";
            CurrentView = new UC_Dashboard();
            _ = SetupDashboardStatsAsync();
        }
        [RelayCommand]
        private void OpenUser()
        {
            SelectedMenuItem = "Kullanıcılar";
            CurrentView = new UC_User();
        }
        [RelayCommand]
        private void OpenLog()
        {
            SelectedMenuItem = "İşlemler";
            CurrentView = new UC_Log();
        }
        [RelayCommand]
        private void OpenDevice()
        {
            SelectedMenuItem = "Ürünler";
            CurrentView = new UC_Device();
        }
        [RelayCommand]
        private void OpenSetting()
        {
            SelectedMenuItem = "Ayarlar";
            CurrentView = new UC_Setting();
        }

        private async Task SetupDashboardStatsAsync()
        {
            var dbService = new DatabaseService();

            int deviceCount = await dbService.GetTotalDeviceCountAsync();
            int userCount = await dbService.GetActiveUserCountAsync();
            int logCount = await dbService.GetTotalLogCountAsync();
            var weeklyData = await dbService.GetWeeklyActivityAsync();
            int haftalikIslemSayisi = weeklyData.Values.Sum();

            StatBox1 = deviceCount.ToString();
            StatBox2 = userCount.ToString();
            StatBox3 = logCount.ToString();
            StatBox4 = haftalikIslemSayisi.ToString();

            var logs = await dbService.GetRecentLogsAsync(50);
            RecentLogs = new ObservableCollection<Log>(logs);

            WeeklyLabels = weeklyData.Keys.ToList();
            var values = new ChartValues<int>(weeklyData.Values);

            WeeklySeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Yapılan İşlem",
                    Values = values,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BB2248")),
                    MaxColumnWidth = 40
                }
            };

            YFormatter = value => value.ToString("N0");
        }

        [RelayCommand]
        private async Task ChangeChartPeriod(string period)
        {
            var dbService = new DatabaseService();
            Dictionary<string, int> data = new();

            if (period == "Week")
            {
                ChartTitle = "Son 7 Günlük İşlem Analizi";
                data = await dbService.GetWeeklyActivityAsync();
            }
            else if (period == "Month")
            {
                ChartTitle = "Son 30 Günlük İşlem Analizi";
                data = await dbService.GetMonthlyActivityAsync();
            }
            else if (period == "Year")
            {
                ChartTitle = "Son 12 Aylık İşlem Analizi";
                data = await dbService.GetYearlyActivityAsync();
            }
            WeeklyLabels = data.Keys.ToList();
            var values = new ChartValues<int>(data.Values);
            WeeklySeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Yapılan İşlem",
                    Values = values,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BB2248")),
                    MaxColumnWidth = 40
                }
            };
        }

        [RelayCommand]
        private async Task Logout()
        {
            bool result = Views.CustomMessageBoxWindow.Show("Sistemden çıkış yapmak istediğinize emin misiniz?", "Çıkış Onayı", Views.CustomMessageBoxType.Info, Views.CustomMessageBoxButtons.YesNo);

            if (result)
            {
                int currentUserId = SessionService.ActiveUserId;

                if (currentUserId > 0)
                {
                    var dbService = new DatabaseService();
                    await dbService.LogActionAsync(currentUserId, IslemTuru.SistemdenCikis);
                }

                SessionService.ActiveUserId = 0;
                var loginWindow = new Views.MainWindow();
                loginWindow.Show();

                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window != loginWindow)
                    {
                        window.Close();
                    }
                }
            }
        }
    }
}
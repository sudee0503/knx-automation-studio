using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sude.Models;
using Sude.Services;
using Sude.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sude.ViewModels.UserViewModels
{
    public partial class DeviceSelectionViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        private readonly MainViewModel _mainRouter;

        [ObservableProperty]
        private ObservableCollection<Device> _devices = new();

        [ObservableProperty]
        private Device _selectedDevice;

        [ObservableProperty]
        private ImageSource _selectedDeviceImage;

        public DeviceSelectionViewModel(MainViewModel mainRouter = null)
        {
            _mainRouter = mainRouter;
            _dbService = new DatabaseService();

            _ = LoadDevicesAsync();
        }

        private async Task LoadDevicesAsync()
        {
            var data = await _dbService.GetDevicesAsync();
            Devices = new ObservableCollection<Device>(data);
        }

        partial void OnSelectedDeviceChanged(Device value)
        {
            if (value != null && value.MainImageFileData != null)
            {
                SelectedDeviceImage = ByteArrayToImage(value.MainImageFileData);
            }
            else
            {
                SelectedDeviceImage = null;
            }
        }

        private ImageSource ByteArrayToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

            try
            {
                using var ms = new MemoryStream(imageData);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }

        [RelayCommand]
        private void ContinueToControl()
        {
            if (SelectedDevice == null)
            {
                Views.CustomMessageBoxWindow.Show("Lütfen test etmek için bir cihaz seçin.", "Uyarı", Views.CustomMessageBoxType.Warning);
                return;
            }

            _mainRouter.Navigate(new DeviceControlerViewModel(_mainRouter, SelectedDevice));
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
                    await _dbService.LogActionAsync(currentUserId, IslemTuru.SistemdenCikis);
                }

                SessionService.ActiveUserId = 0;
                _mainRouter.Navigate(new LoginViewModel(_mainRouter));
            }
        }
    }
}
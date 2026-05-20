using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sude.Models;
using Sude.Services;
using Sude.Views.AdminViews;
using Sude.Views.AdminViews.DialogViews;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Sude.ViewModels.AdminViewModels
{
    public partial class DeviceViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService = new DatabaseService();

        [ObservableProperty]
        private ObservableCollection<Device> _devices = new();

        public DeviceViewModel()
        {
            _ = LoadDevicesAsync();
        }

        private async Task LoadDevicesAsync()
        {
            var data = await _dbService.GetDevicesAsync();
            Devices = new ObservableCollection<Device>(data);
        }

        [RelayCommand]
        private async Task AddDevice()
        {
            var window = new DialogWindow(new DeviceAdd());
            window.ShowDialog();
            await LoadDevicesAsync();
        }

        [RelayCommand]
        private async Task EditDevice(Device selectedDevice)
        {
            if (selectedDevice == null) return;

            var window = new DialogWindow(new DeviceEdit(selectedDevice));
            window.ShowDialog();
            await LoadDevicesAsync();
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sude.Models;
using Sude.Services;
using Sude.Views;
using Sude.Views.AdminViews.DialogViews;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Sude.ViewModels.AdminViewModels.DialogWindowViewModels
{
    public partial class TestStepItem : ObservableObject
    {
        [ObservableProperty] private int _stepOrder;
        [ObservableProperty] private string _instructionText;
        [ObservableProperty] private string _imagePath;

        private readonly Action<TestStepItem> _removeCallback;

        public TestStepItem(int order, Action<TestStepItem> removeCallback)
        {
            StepOrder = order;
            _removeCallback = removeCallback;
        }

        [RelayCommand]
        private void SelectImage()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png" };
            if (ofd.ShowDialog() == true)
            {
                string ext = Path.GetExtension(ofd.FileName).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    CustomMessageBoxWindow.Show("Lütfen sadece geçerli bir resim dosyası (.jpg, .png) seçin!", "Hatalı Dosya Türü", CustomMessageBoxType.Warning);
                    return;
                }
                ImagePath = ofd.FileName;
            }
        }

        [RelayCommand]
        private void Remove() => _removeCallback?.Invoke(this);
    }

    public partial class DeviceAddViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly FileService _fileService;
        public Action CloseAction { get; set; }

        [ObservableProperty] private int? _deviceId;
        [ObservableProperty] private string _deviceType;
        [ObservableProperty] private string _selectedProjectPath;
        [ObservableProperty] private string _selectedVideoPath;
        [ObservableProperty] private string _mainImagePath;

        public ObservableCollection<TestStepItem> TestSteps { get; } = new();

        public DeviceAddViewModel()
        {
            _databaseService = new DatabaseService();
            _fileService = new FileService();
        }

        [RelayCommand]
        private void SelectProject()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "EITT Proje Dosyası|*.ctl" };
            if (ofd.ShowDialog() == true)
            {
                string ext = Path.GetExtension(ofd.FileName).ToLower();
                if (ext != ".ctl")
                {
                    CustomMessageBoxWindow.Show("Cihaz projesi için yalnızca (.ctl) uzantılı EITT dosyası seçebilirsiniz!", "Hatalı Dosya Türü", CustomMessageBoxType.Warning);
                    return;
                }
                SelectedProjectPath = ofd.FileName;
            }
        }

        [RelayCommand]
        private void SelectVideo()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "Video Dosyaları|*.mp4;*.avi;*.mkv" };
            if (ofd.ShowDialog() == true)
            {
                string ext = Path.GetExtension(ofd.FileName).ToLower();
                if (ext != ".mp4" && ext != ".avi" && ext != ".mkv")
                {
                    CustomMessageBoxWindow.Show("Lütfen geçerli bir video dosyası (.mp4, .avi) seçin!", "Hatalı Dosya Türü", CustomMessageBoxType.Warning);
                    return;
                }
                SelectedVideoPath = ofd.FileName;
            }
        }

        [RelayCommand]
        private void SelectMainImage()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png" };
            if (ofd.ShowDialog() == true)
            {
                string ext = Path.GetExtension(ofd.FileName).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    CustomMessageBoxWindow.Show("Cihaz ana görseli için sadece resim dosyası (.jpg, .png) seçebilirsiniz!", "Hatalı Dosya Türü", CustomMessageBoxType.Warning);
                    return;
                }
                MainImagePath = ofd.FileName;
            }
        }

        [RelayCommand]
        private void AddNewStep()
        {
            int newOrder = TestSteps.Count + 1;
            TestSteps.Add(new TestStepItem(newOrder, RemoveStep));
        }

        private void RemoveStep(TestStepItem stepToRemove)
        {
            TestSteps.Remove(stepToRemove);
            for (int i = 0; i < TestSteps.Count; i++)
            {
                TestSteps[i].StepOrder = i + 1;
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                if (DeviceId == null || string.IsNullOrWhiteSpace(DeviceType) || string.IsNullOrWhiteSpace(MainImagePath) || string.IsNullOrWhiteSpace(SelectedProjectPath))
                {
                    CustomMessageBoxWindow.Show("Lütfen yıldızlı (*) tüm zorunlu alanları doldurun!", "Eksik Bilgi", CustomMessageBoxType.Warning);
                    return;
                }

                if (DeviceId <= 0 || DeviceId > 255)
                {
                    CustomMessageBoxWindow.Show("Seri numarası mantığı gereği Cihaz ID değeri 1 ile 255 arasında olmalıdır!", "Kural İhlali", CustomMessageBoxType.Error);
                    return;
                }

                bool idExists = await _databaseService.IsDeviceIdExistsAsync(DeviceId.Value);
                if (idExists)
                {
                    CustomMessageBoxWindow.Show($"Girdiğiniz {DeviceId.Value} numaralı ID zaten veritabanında mevcut. Lütfen farklı bir ID girin.", "Çakışma", CustomMessageBoxType.Error);
                    return;
                }

                var device = new Device
                {
                    Id = DeviceId.Value,
                    DeviceType = DeviceType,
                    ProjectFileName = _fileService.GetFileName(SelectedProjectPath),
                    VideoFileName = _fileService.GetFileName(SelectedVideoPath),
                    ProjectFileData = _fileService.GetFileBytes(SelectedProjectPath),
                    VideoFileData = _fileService.GetFileBytes(SelectedVideoPath),
                    MainImageFileData = _fileService.GetFileBytes(MainImagePath)
                };

                var assets = new List<DeviceAsset>();

                foreach (var step in TestSteps)
                {
                    if (!string.IsNullOrWhiteSpace(step.InstructionText))
                    {
                        assets.Add(new DeviceAsset
                        {
                            StepOrder = step.StepOrder,
                            ContentType = "InstructionText",
                            ContentText = step.InstructionText
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(step.ImagePath))
                    {
                        assets.Add(new DeviceAsset
                        {
                            StepOrder = step.StepOrder,
                            ContentType = "ButtonImage",
                            ContentData = _fileService.GetFileBytes(step.ImagePath)
                        });
                    }
                }

                int newDeviceId = await _databaseService.AddDeviceWithAssetsAsync(device, assets);
                await _databaseService.LogActionAsync(SessionService.ActiveUserId, IslemTuru.YeniCihazEklendi, targetDeviceId: newDeviceId);

                CustomMessageBoxWindow.Show("Cihaz ve dinamik test adımları başarıyla kaydedildi!", "Başarılı", CustomMessageBoxType.Success);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"Kayıt sırasında hata:\n{ex.Message}", "Hata", CustomMessageBoxType.Error);
            }
        }

        [RelayCommand]
        private void Cancel() => CloseAction?.Invoke();
    }
}
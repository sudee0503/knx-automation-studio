using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sude.Models;
using Sude.Services;
using Sude.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sude.ViewModels.AdminViewModels.DialogWindowViewModels
{
    public partial class EditStepItem : ObservableObject
    {
        [ObservableProperty] 
        private int _stepOrder;
        
        [ObservableProperty] 
        private string _instructionText;

        [ObservableProperty] 
        private string _imagePath;

        [ObservableProperty] 
        private byte[] _existingImageBytes;

        [ObservableProperty] 
        private ImageSource _displayImage;

        private readonly Action<EditStepItem> _removeCallback;

        public EditStepItem(int order, Action<EditStepItem> removeCallback)
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
                    CustomMessageBoxWindow.Show("Lütfen sadece resim dosyası seçin!", "Uyarı", CustomMessageBoxType.Warning);
                    return;
                }

                ImagePath = ofd.FileName;
                DisplayImage = new BitmapImage(new Uri(ImagePath));
            }
        }

        [RelayCommand]
        private void Remove() => _removeCallback?.Invoke(this);
    }
    public partial class DeviceEditViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly FileService _fileService;

        private string _newProjectPath;
        private string _newVideoPath;
        private string _newMainImagePath;

        [ObservableProperty] 
        private Device _editingDevice;
        
        [ObservableProperty] 
        private int _deviceId;
        
        [ObservableProperty] 
        private string _deviceType;

        [ObservableProperty] 
        private string _projectDisplay;
        
        [ObservableProperty] 
        private string _videoDisplay;
        
        [ObservableProperty] 
        private string _mainImageDisplay;

        private bool _isVideoDeleted = false;

        public Action CloseAction { get; set; }

        public ObservableCollection<EditStepItem> TestSteps { get; } = new();

        public DeviceEditViewModel(Device selectedDevice)
        {
            _databaseService = new DatabaseService();
            _fileService = new FileService();

            EditingDevice = selectedDevice;
            DeviceId = selectedDevice.Id;
            DeviceType = selectedDevice.DeviceType;

            ProjectDisplay = selectedDevice.ProjectFileName ?? "Proje yüklenmemiş";
            VideoDisplay = selectedDevice.VideoFileName ?? "Video yüklenmemiş";
            MainImageDisplay = "Veritabanında kayıtlı ana görsel";

            _ = LoadExistingAssetsAsync();
        }
        private async Task LoadExistingAssetsAsync()
        {
            var assets = await _databaseService.GetDeviceAssetsAsync(DeviceId);

            var groupedAssets = assets.GroupBy(a => a.StepOrder).OrderBy(g => g.Key);

            foreach (var group in groupedAssets)
            {
                var step = new EditStepItem(group.Key, RemoveStep);

                var textAsset = group.FirstOrDefault(a => a.ContentType == "InstructionText");
                if (textAsset != null) step.InstructionText = textAsset.ContentText;

                var imageAsset = group.FirstOrDefault(a => a.ContentType == "ButtonImage");
                if (imageAsset != null)
                {
                    step.ExistingImageBytes = imageAsset.ContentData;
                    step.DisplayImage = ByteArrayToImage(imageAsset.ContentData);
                }
                TestSteps.Add(step);
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
            catch { return null; }
        }

        [RelayCommand]
        private void SelectProject()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "EITT Proje|*.ctl" };
            if (ofd.ShowDialog() == true)
            {
                if (Path.GetExtension(ofd.FileName).ToLower() != ".ctl") return;
                _newProjectPath = ofd.FileName;
                ProjectDisplay = ofd.FileName;
            }
        }

        [RelayCommand]
        private void SelectVideo()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "Video|*.mp4;*.avi" };
            if (ofd.ShowDialog() == true)
            {
                _newVideoPath = ofd.FileName;
                VideoDisplay = ofd.FileName;
            }
        }

        [RelayCommand]
        private void SelectMainImage()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "Resim|*.jpg;*.png" };
            if (ofd.ShowDialog() == true)
            {
                _newMainImagePath = ofd.FileName;
                MainImageDisplay = ofd.FileName;
            }
        }

        [RelayCommand]
        private void AddNewStep()
        {
            int newOrder = TestSteps.Count + 1;
            TestSteps.Add(new EditStepItem(newOrder, RemoveStep));
        }

        private void RemoveStep(EditStepItem stepToRemove)
        {
            TestSteps.Remove(stepToRemove);
            for (int i = 0; i < TestSteps.Count; i++)
            {
                TestSteps[i].StepOrder = i + 1;
            }
        }

        [RelayCommand]
        private async Task Update()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(DeviceType))
                {
                    CustomMessageBoxWindow.Show("Cihaz Adı boş bırakılamaz.", "Uyarı", CustomMessageBoxType.Error);
                    return;
                }

                EditingDevice.DeviceType = DeviceType;

                if (!string.IsNullOrEmpty(_newProjectPath))
                {
                    EditingDevice.ProjectFileName = _fileService.GetFileName(_newProjectPath);
                    EditingDevice.ProjectFileData = _fileService.GetFileBytes(_newProjectPath);
                }

                if (_isVideoDeleted)
                {
                    EditingDevice.VideoFileName = null;
                    EditingDevice.VideoFileData = null;
                }
                else if (!string.IsNullOrEmpty(_newVideoPath))
                {
                    EditingDevice.VideoFileName = _fileService.GetFileName(_newVideoPath);
                    EditingDevice.VideoFileData = _fileService.GetFileBytes(_newVideoPath);
                }

                if (!string.IsNullOrEmpty(_newMainImagePath))
                {
                    EditingDevice.MainImageFileData = _fileService.GetFileBytes(_newMainImagePath);
                }
                var newAssets = new List<DeviceAsset>();

                foreach (var step in TestSteps)
                {
                    if (!string.IsNullOrWhiteSpace(step.InstructionText))
                    {
                        newAssets.Add(new DeviceAsset
                        {
                            StepOrder = step.StepOrder,
                            ContentType = "InstructionText",
                            ContentText = step.InstructionText
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(step.ImagePath))
                    {
                        newAssets.Add(new DeviceAsset
                        {
                            StepOrder = step.StepOrder,
                            ContentType = "ButtonImage",
                            ContentData = _fileService.GetFileBytes(step.ImagePath)
                        });
                    }
                    else if (step is EditStepItem editStep && editStep.ExistingImageBytes != null)
                    {
                        newAssets.Add(new DeviceAsset
                        {
                            StepOrder = step.StepOrder,
                            ContentType = "ButtonImage",
                            ContentData = editStep.ExistingImageBytes
                        });
                    }
                }

                await _databaseService.UpdateDeviceAsync(EditingDevice, newAssets);
                await _databaseService.LogActionAsync(SessionService.ActiveUserId, IslemTuru.CihazGuncellendi, targetDeviceId: EditingDevice.Id);

                CustomMessageBoxWindow.Show("Cihaz ve test senaryosu başarıyla güncellendi!", "Başarılı", CustomMessageBoxType.Success);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"Güncelleme sırasında hata:\n{ex.Message}", "Hata", CustomMessageBoxType.Error);
            }
        }

        [RelayCommand]
        private void DeleteVideo()
        {
            _isVideoDeleted = true;
            _newVideoPath = null;
            VideoDisplay = "Video kaldırıldı (Kaydedince silinecek)";
        }

        [RelayCommand]
        private void Cancel() => CloseAction?.Invoke();
    }
}
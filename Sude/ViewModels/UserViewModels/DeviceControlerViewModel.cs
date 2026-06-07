using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sude.Models;
using Sude.Services;
using Sude.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sude.ViewModels.UserViewModels
{
    public partial class DeviceControlerViewModel : ObservableObject
    {
        private readonly MainViewModel _mainRouter;
        private readonly EittService _eittService;
        private readonly DatabaseService _dbService;
        private string _tempVideoPath;
        private string _tempCtlPath;
        private int sFirmCode = 726;
        private List<DeviceAsset> _deviceAssets = new();

        [ObservableProperty]
        private Device _currentDevice;

        [ObservableProperty]
        private ImageSource _displayImage;

        [ObservableProperty]
        private string _instructionText = "EITT Başlatılıyor... Lütfen Bekleyin.";

        [ObservableProperty]
        private bool _isEittReady = false;

        [ObservableProperty]
        private bool _isTestRunning = false;

        [ObservableProperty]
        private bool _isVideoOverlayVisible = true;

        [ObservableProperty]
        private string _deviceVideoPath;

        public DeviceControlerViewModel(MainViewModel mainRouter, Device selectedDevice)
        {
            _mainRouter = mainRouter;
            _eittService = new EittService();
            _dbService = new DatabaseService();

            CurrentDevice = selectedDevice;

            if (selectedDevice.MainImageFileData != null)
                DisplayImage = ByteArrayToImage(selectedDevice.MainImageFileData);

            if (CurrentDevice.VideoFileData != null && CurrentDevice.VideoFileData.Length > 0)
            {
                try
                {
                    _tempVideoPath = Path.Combine(Path.GetTempPath(), $"temp_video_{CurrentDevice.Id}.mp4");
                    File.WriteAllBytes(_tempVideoPath, CurrentDevice.VideoFileData);
                    DeviceVideoPath = _tempVideoPath;
                    IsVideoOverlayVisible = true;
                }
                catch
                {
                    IsVideoOverlayVisible = false;
                }
            }
            else
            {
                IsVideoOverlayVisible = false;
            }

            _ = InitializeEittAsync();
        }

        [RelayCommand]
        private void CloseVideo()
        {
            IsVideoOverlayVisible = false;
        }

        private async Task InitializeEittAsync()
        {
            if (CurrentDevice.ProjectFileData == null)
            {
                InstructionText = "HATA: Veritabanında cihaza ait proje dosyası yok!";
                return;
            }

            try
            {
                _tempCtlPath = Path.Combine(Path.GetTempPath(), CurrentDevice.ProjectFileName ?? "temp_project.ctl");
                await File.WriteAllBytesAsync(_tempCtlPath, CurrentDevice.ProjectFileData);

                await Task.Run(() =>
                {
                    if (!_eittService.OpenProject(_tempCtlPath)) throw new Exception("Proje açılamadı.");
                });
                _deviceAssets = await _dbService.GetDeviceAssetsAsync(CurrentDevice.Id);

                InstructionText = "Cihaz Bağlantısı Hazır! Test Et butonuna basabilirsiniz.";
                IsEittReady = true;
            }
            catch (Exception ex)
            {
                InstructionText = "EITT Bağlantı Hatası: " + ex.Message;
            }
        }

        [RelayCommand]
        private async Task TestDevice()
        {
            IsTestRunning = true;
            IsEittReady = false;

            try
            {
                dynamic seqManager = _eittService.CurrentProject.GetSequenceManager();
                dynamic serialSeq = seqManager.GetFirstSequence();
                dynamic controlSeq = seqManager.GetNextSequence();

                _eittService.CurrentProject.GetTraceBuffer().Clear();
                controlSeq.ResetEvaluationError();
                controlSeq.SetErrorStop(false);

                int totalSteps = controlSeq.GetItemNumber();
                int currentStep = 1;
                bool testRunning = true;

                int initialTraceCount = _eittService.CurrentProject.GetTraceBuffer().GetTelegramNumber();

                UpdateUI(currentStep);
                controlSeq.Start();

                while (testRunning)
                {
                    int evalError = Convert.ToInt32(controlSeq.EvaluationError());
                    if (evalError != 0)
                    {
                        StopWithError(controlSeq, "Yanlış butona basıldı veya telegram hatası!");
                        return;
                    }

                    int currentTraceCount = _eittService.CurrentProject.GetTraceBuffer().GetTelegramNumber();
                    int systemTelegrams = 3;
                    int processedCount = (currentTraceCount - initialTraceCount) - systemTelegrams;

                    if (processedCount < 0) processedCount = 0;

                    if (processedCount >= currentStep)
                    {
                        currentStep = processedCount + 1;

                        if (currentStep > totalSteps)
                        {
                            if (Convert.ToInt32(controlSeq.IsRunning()) == 0)
                            {
                                testRunning = false;
                                await FinishControl(serialSeq);
                                return;
                            }
                        }
                        else
                        {
                            UpdateUI(currentStep);
                        }
                    }

                    if (Convert.ToInt32(controlSeq.IsRunning()) == 0 && currentStep <= totalSteps)
                    {
                        testRunning = false;
                        await FinishControl(serialSeq);
                        return;
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                InstructionText = "Test sırasında hata: " + ex.Message;
                IsTestRunning = false;
                IsEittReady = true;
            }
        }

        private void UpdateUI(int step)
        {
            if (step == 1)
            {
                InstructionText = "Cihaz resetleniyor...";
                if (CurrentDevice.MainImageFileData != null)
                    DisplayImage = ByteArrayToImage(CurrentDevice.MainImageFileData);
            }
            else
            {
                int testStep = step - 1;
                var stepAssets = _deviceAssets.Where(a => a.StepOrder == testStep).ToList();

                if (stepAssets.Any())
                {
                    var combinedAsset = stepAssets.FirstOrDefault(a => a.ContentType == "StepData");

                    if (combinedAsset != null)
                    {
                        InstructionText = !string.IsNullOrWhiteSpace(combinedAsset.ContentText) ? combinedAsset.ContentText : $"Lütfen cihazı tetikleyin ({testStep}. Adım)";
                        DisplayImage = combinedAsset.ContentData != null ? ByteArrayToImage(combinedAsset.ContentData) : ByteArrayToImage(CurrentDevice.MainImageFileData);
                    }
                }
                else
                {
                    DisplayImage = ByteArrayToImage(CurrentDevice.MainImageFileData);
                    InstructionText = $"Lütfen cihazı tetikleyin ({testStep}. Adım)";
                }
            }
        }

        private void StopWithError(dynamic controlSeq, string message)
        {
            controlSeq.Stop();
            IsTestRunning = false;
            IsEittReady = true;

            _ = _dbService.LogActionAsync(SessionService.ActiveUserId, IslemTuru.CihazKontrolHatali, targetDeviceId: CurrentDevice.Id);

            InstructionText = "Test Başarısız: " + message;
            CustomMessageBoxWindow.Show(message, "Test Başarısız", CustomMessageBoxType.Error);
        }

        private async Task FinishControl(dynamic serialSeq)
        {
            IsTestRunning = false;
            InstructionText = "Kontrol tamamlandı, seri numara atanıyor...";

            await _dbService.LogActionAsync(SessionService.ActiveUserId, IslemTuru.CihazKontrolBasarili, targetDeviceId: CurrentDevice.Id);

            await RunSerialSequence(serialSeq);
            IsEittReady = true;
        }

        private async Task RunSerialSequence(dynamic serialSeq)
        {
            try
            {
                var (newId, serialBytes) = _dbService.NewSerial(CurrentDevice.Id, sFirmCode);
                string serialStr = BitConverter.ToString(serialBytes).Replace("-", "");

                if (!PrepareSerialSequence(serialSeq, serialBytes)) return;

                _eittService.EittApp.StartCommunication();
                serialSeq.StartSequence();

                while (Convert.ToInt32(serialSeq.IsRunning()) != 0)
                {
                    await Task.Delay(100);
                }

                bool hasError = ((short)serialSeq.EvaluationError()) != 0;
                _dbService.UpdateAssigned(newId, !hasError);

                if (!hasError)
                {
                    await _dbService.LogActionAsync(SessionService.ActiveUserId, IslemTuru.CihazIdAtandi, targetDeviceId: CurrentDevice.Id, hedefSeriNo: serialStr);
                    InstructionText = $"SERİ ATAMA BAŞARILI! ({serialStr})";
                    CustomMessageBoxWindow.Show($"Cihaz başarıyla kodlandı.\nSeri No: {serialStr}", "Başarılı", CustomMessageBoxType.Success);
                }
                else
                {
                    await _dbService.LogActionAsync(SessionService.ActiveUserId,
                                                        IslemTuru.CihazIdAtamaHatasi,
                                                        targetDeviceId: CurrentDevice.Id,
                                                        hedefSeriNo: serialStr);
                    InstructionText = "Seri atama başarısız!";
                    CustomMessageBoxWindow.Show("Seri atama başarısız oldu!", "Hata", CustomMessageBoxType.Error);
                }
            }
            catch (Exception ex)
            {
                InstructionText = "Seri aşamasında hata: " + ex.Message;
                CustomMessageBoxWindow.Show("Seri atama hatası: " + ex.Message, "Hata", CustomMessageBoxType.Error);
            }
        }

        private bool PrepareSerialSequence(dynamic serialSeq, byte[] serialBytes)
        {
            try
            {
                var tel4 = serialSeq.GetTelegram(4);
                var tel6 = serialSeq.GetTelegram(6);
                var tel7 = serialSeq.GetTelegram(7);

                UpdateTelegram(tel4, serialBytes);
                UpdateTelegram(tel6, serialBytes);
                UpdateTelegram(tel7, serialBytes, true);
                return true;
            }
            catch { return false; }
        }

        private void UpdateTelegram(dynamic tel, byte[] serial, bool excludeLast4 = false)
        {
            if (serial == null || serial.Length != 6)
                throw new InvalidOperationException("Geçerli seri numarası verisi eksik!");

            string csvData = tel.GetDataAsCSV();
            var parsed = ParseCsv(csvData);

            if (!excludeLast4)
            {
                for (int i = 0; i < 6; i++)
                {
                    parsed[parsed.Count - 6 + i] = parsed[parsed.Count - 6 + i].Split('=')[0] + "=" + serial[i].ToString("X2");
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    parsed[parsed.Count - 10 + i] = parsed[parsed.Count - 10 + i].Split('=')[0] + "=" + serial[i].ToString("X2");
                }
            }

            string newCsv = SafeJoin(parsed);
            tel.SetDataFromCSV(newCsv);
        }

        private List<string> ParseCsv(string input)
        {
            var result = new List<string>();
            bool insideQuotes = false;
            string current = "";
            int counter = 1;

            foreach (char c in input)
            {
                if (c == '"')
                {
                    insideQuotes = !insideQuotes;
                    current += c;
                }
                else if (c == ';' && !insideQuotes)
                {
                    result.Add($"parse{counter}={current}");
                    counter++;
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            if (current.Length > 0) result.Add(current);
            return result;
        }

        private string SafeJoin(List<string> parsed)
        {
            return string.Join(";", parsed.Select(x =>
            {
                int eq = x.IndexOf('=');
                return eq >= 0 ? x.Substring(eq + 1) : "";
            }));
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
        private void GoBack()
        {
            if (IsTestRunning)
            {
                CustomMessageBoxWindow.Show("Test devam ederken çıkış yapamazsınız!", "Uyarı", CustomMessageBoxType.Warning);
                return;
            }
            _eittService.CloseCurrentProject();

            if (!string.IsNullOrEmpty(_tempCtlPath) && File.Exists(_tempCtlPath))
                File.Delete(_tempCtlPath);

            _mainRouter.Navigate(new DeviceSelectionViewModel(_mainRouter));
        }

        [RelayCommand]
        private async Task Logout()
        {
            bool result = CustomMessageBoxWindow.Show("Sistemden çıkış yapmak istediğinize emin misiniz?", "Çıkış Onayı", CustomMessageBoxType.Info, CustomMessageBoxButtons.YesNo);

            if (result)
            {
                if (IsTestRunning)
                {
                    CustomMessageBoxWindow.Show("Test devam ederken çıkış yapamazsınız! Lütfen önce testi bitirin.", "Uyarı", CustomMessageBoxType.Warning);
                    return;
                }

                int currentUserId = SessionService.ActiveUserId;
                if (currentUserId > 0)
                {
                    await _dbService.LogActionAsync(currentUserId, IslemTuru.SistemdenCikis);
                }

                SessionService.ActiveUserId = 0;
                _mainRouter.Navigate(new LoginViewModel(_mainRouter));

                string pathToDelete = _tempCtlPath;
                _ = Task.Run(() =>
                {
                    _eittService.ReleaseEitt();
                    if (!string.IsNullOrEmpty(pathToDelete) && File.Exists(pathToDelete))
                    {
                        try { File.Delete(pathToDelete); } catch { }
                    }
                });
            }
        }
    }
}
using System;
using System.Runtime.InteropServices;
using Sude.Views;

namespace Sude.Services
{
    public class EittService
    {
        private static dynamic _eittAppInstance;
        private static dynamic _currentProjectInstance;

        public dynamic EittApp
        {
            get => _eittAppInstance;
            private set => _eittAppInstance = value;
        }

        public dynamic CurrentProject
        {
            get => _currentProjectInstance;
            private set => _currentProjectInstance = value;
        }

        public bool InitEitt()
        {
            if (EittApp != null)
            {
                return true;
            }

            try
            {
                Type t = Type.GetTypeFromProgID("Eitt.Application");
                if (t == null)
                {
                    CustomMessageBoxWindow.Show("EITT yazılımı bu bilgisayarda yüklü değil!", "Hata", CustomMessageBoxType.Error);
                    return false;
                }

                EittApp = Activator.CreateInstance(t);

                ForceWpfToFront();

                try { EittApp.Visible = false; } catch { }

                EittApp.EnableAutomatedMode(false);
                EittApp.ShowCommentCommandDialogs(false);

                return true;
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("EITT başlatma hatası: " + ex.Message, "Hata", CustomMessageBoxType.Error);
                ReleaseEitt();
                return false;
            }
        }
        public bool OpenProject(string filePath)
        {
            if (EittApp == null)
            {
                if (!InitEitt()) return false;
            }
            try
            {
                CurrentProject = EittApp.OpenProjectFile(filePath);
                if (CurrentProject == null)
                {
                    CustomMessageBoxWindow.Show($"Proje açılamadı: {filePath}", "Hata", CustomMessageBoxType.Error);
                    return false;
                }
                ForceWpfToFront();
                try { EittApp.Visible = false; } catch { }

                CurrentProject.ActivateBusConnectionList();
                EittApp.StartCommunication();

                return true;
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"Proje açma hatası: {ex.Message}", "Hata", CustomMessageBoxType.Error);
                CloseCurrentProject();
                return false;
            }
        }
        private void ForceWpfToFront()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = System.Windows.Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Topmost = true;  
                    mainWindow.Activate();      
                    mainWindow.Focus();         
                    mainWindow.Topmost = false; 
                }
            });
        }
        public void CloseCurrentProject()
        {
            if (CurrentProject != null)
            {
                try
                {
                    if (EittApp != null)
                    {
                        EittApp.StopCommunication();
                    }

                    System.Threading.Thread.Sleep(500);

                    CurrentProject.Close();
                    Marshal.ReleaseComObject(CurrentProject);
                }
                catch { }
                finally
                {
                    CurrentProject = null;
                }
            }
        }
        public void ReleaseEitt()
        {
            CloseCurrentProject();
            if (EittApp != null)
            {
                try
                {
                    Marshal.ReleaseComObject(EittApp);
                }
                catch {  }
                finally
                {
                    EittApp = null;
                }
            }
        }
    }
}
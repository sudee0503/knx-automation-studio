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
    public partial class UserViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService = new DatabaseService();

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        public UserViewModel()
        {
            _ = LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            var data = await _dbService.GetUsersAsync();
            Users = new ObservableCollection<User>(data);
        }

        [RelayCommand]
        private async Task AddUser()
        {
            var window = new DialogWindow(new UserAdd());
            window.ShowDialog();

            await LoadUsersAsync();
        }

        [RelayCommand]
        private async Task EditUser(User selectedUser)
        {
            if (selectedUser == null) return;

            var window = new DialogWindow(new UserEdit(selectedUser));
            window.ShowDialog();

            await LoadUsersAsync();
        }
        [RelayCommand]
        private async Task DeleteUser(User user)
        {
            if (user == null) return;
            if (user.Id == SessionService.ActiveUserId)
            {
                Views.CustomMessageBoxWindow.Show("Şu anda aktif olarak kullandığınız kendi hesabınızı silemezsiniz!", "İşlem Reddedildi", Views.CustomMessageBoxType.Warning);
                return;
            }
            if (user.Role == "Admin")
            {
                int adminCount = await _dbService.GetActiveAdminCountAsync();

                if (adminCount <= 1)
                {
                    Views.CustomMessageBoxWindow.Show("Sistemdeki tek yönetici hesabını silemezsiniz! İşleme devam etmek için lütfen önce sisteme başka bir Admin ekleyin.", "İşlem Reddedildi", Views.CustomMessageBoxType.Error);
                    return;
                }
            }
            bool result = Views.CustomMessageBoxWindow.Show($"{user.Username} isimli kullanıcıyı silmek istediğinize emin misiniz?", "Silme Onayı", Views.CustomMessageBoxType.Warning, Views.CustomMessageBoxButtons.YesNo);
            if (result)
            {
                int currentAdminId = SessionService.ActiveUserId;
                await _dbService.SoftDeleteUserAsync(user.Id, currentAdminId);
                Users.Remove(user);
            }
        }
    }
}
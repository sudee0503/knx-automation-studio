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
using Sude.Services;

namespace Sude.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentView;

        public MainViewModel()
        {
            CurrentView = new LoginViewModel(this);
        }

        public void Navigate(object viewModel)
        {
            CurrentView = viewModel;
        }
    }
}
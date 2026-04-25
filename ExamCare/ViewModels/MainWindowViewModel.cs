using CommunityToolkit.Mvvm.ComponentModel;
using ExamCare.Views;
using System;
using ExamCare.Services;

namespace ExamCare.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly StudentExamApiService _apiService;
        public Action<bool>? FullscreenAction { get; set; }
        public Action? CloseAction { get; set; }

        [ObservableProperty]
        private object? currentView;

       

        public MainWindowViewModel(StudentExamApiService apiService)
        {
            _apiService = apiService;
        }

        public void Init()
        {
            CurrentView = new LoginView
            {
                DataContext = new LoginViewModel(this, _apiService)
            };
        }

        public void ShowDashboard()
        {
            FullscreenAction?.Invoke(true);
            CurrentView = new DashboardView
            {
                DataContext = new DashboardViewModel(
                    _apiService,
                    () => { CloseAction?.Invoke(); }
                )
            };
        }
    }
}
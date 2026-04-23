using CommunityToolkit.Mvvm.ComponentModel;
using ExamCare.Views;
using System;
using ExamCare.Services;

namespace ExamCare.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly StudentExamApiService _apiService;

        [ObservableProperty]
        private object? currentView;

        public Action? CloseAction { get; set; }

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
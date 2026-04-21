using Avalonia.Controls;
using ExamCare.ViewModels;

namespace ExamCare
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var vm = new MainWindowViewModel();

            // 👉 cho phép ViewModel đóng app
            vm.CloseAction = () => this.Close();

            // 👉 QUAN TRỌNG: truyền vm vào Login
            vm.Init();

            DataContext = vm;
        }
    }
}
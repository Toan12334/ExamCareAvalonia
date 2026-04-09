using Avalonia.Controls;
using ExamCare.ViewModels;
using LiveMarkdown.Avalonia;
namespace ExamCare
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
          

        }
    }
}
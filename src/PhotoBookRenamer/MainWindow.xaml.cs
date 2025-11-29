using PhotoBookRenamer.ViewModels;
using System.Windows;

namespace PhotoBookRenamer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
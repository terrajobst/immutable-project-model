using System.Windows;

using TimephasedExperiments.ViewModels;

namespace TimephasedExperiments
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new AssignmenUsageViewModel();
        }
    }
}

using System.ComponentModel.Composition.Hosting;
using System.Windows;

namespace Demo
{
    internal sealed partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var catalog = new AssemblyCatalog(GetType().Assembly);
            var compositionContainer = new CompositionContainer(catalog);
            var mainWindow = compositionContainer.GetExportedValue<MainWindow>();
            mainWindow.Show();
        }
    }
}

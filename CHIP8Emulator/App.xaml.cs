using System.Windows;

using CHIP8Core;

namespace CHIP8Emulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var chip = new CHIP8();

            chip.Start();
        }
    }
}
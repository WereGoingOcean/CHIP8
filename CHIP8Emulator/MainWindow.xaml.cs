using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using CHIP8Core;

namespace CHIP8Emulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly CHIP8 emulator;

        private readonly Dictionary<Key, byte> ValidKeys = new Dictionary<Key, byte>
                                                           {
                                                               {
                                                                   Key.D1, 0x1
                                                               },
                                                               {
                                                                   Key.D2, 0x2
                                                               },
                                                               {
                                                                   Key.D3, 0x3
                                                               },
                                                               {
                                                                   Key.D4, 0x4
                                                               },
                                                               {
                                                                   Key.D5, 0x5
                                                               },
                                                               {
                                                                   Key.D6, 0x6
                                                               },
                                                               {
                                                                   Key.D7, 0x7
                                                               },
                                                               {
                                                                   Key.D8, 0x8
                                                               },
                                                               {
                                                                   Key.D9, 0x9
                                                               },
                                                               {
                                                                   Key.A, 0xA
                                                               },
                                                               {
                                                                   Key.B, 0xB
                                                               },
                                                               {
                                                                   Key.C, 0xC
                                                               },
                                                               {
                                                                   Key.D, 0xD
                                                               },
                                                               {
                                                                   Key.E, 0xE
                                                               },
                                                               {
                                                                   Key.F, 0xF
                                                               }
                                                           };

        private void DisplayEmulatorScreen(bool[,] pixels)
        {
            Application.Current.Dispatcher.Invoke(() => this.MainCanvas.Children.Clear());

            for(var x = 0; x < pixels.GetLength(0); x++)
            {
                for (var y = 0; y < pixels.GetLength(1); y++)
                {
                    if (pixels[x,
                               y])
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                                                              {
                                                                  var rec = new Rectangle();
                                                                  Canvas.SetTop(rec,
                                                                                y * 4);
                                                                  Canvas.SetLeft(rec,
                                                                                 x * 4);
                                                                  rec.Width = 4;
                                                                  rec.Height = 4;
                                                                  rec.Fill = new SolidColorBrush(Colors.Black);
                                                                  MainCanvas.Children.Add(rec);
                                                              });
                    }
                }
            }
        }

        public MainWindow()
        {
            var registers = new RegisterModule();

            emulator = new CHIP8(DisplayEmulatorScreen,
                                 registers,
                                 new StackModule());

            var bytes = File.ReadAllBytes("test_display_0.ch8");

            emulator.LoadProgram(bytes);

            InitializeComponent();

            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Task.Run(() => emulator.Start());
        }

        protected override void OnClosed(EventArgs e)
        {
            emulator.Stop();
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (ValidKeys.TryGetValue(e.Key, out var byteKey))
            {
                emulator.KeyReleased(byteKey);
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (ValidKeys.TryGetValue(e.Key, out var byteKey))
            {
                emulator.KeyPressed(byteKey);
            }
        }
    }
}
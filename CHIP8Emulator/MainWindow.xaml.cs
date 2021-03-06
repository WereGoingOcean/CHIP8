﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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

        private readonly SemaphoreSlim displayLock = new SemaphoreSlim(1, 1);

        private async Task DisplayEmulatorScreen(bool[,] pixels)
        {
            try
            {
                await displayLock.WaitAsync();

                const int PixelSize = 4;

                var actions = new List<Action>();

                for (var x = 0; x < pixels.GetLength(0); x++)
                {
                    for (var y = 0; y < pixels.GetLength(1); y++)
                    {
                        var point = new Point(x,
                                              y);

                        var currentRect = displayDictionary[point];

                        if (currentRect == null
                            && pixels[x,
                                      y])
                        {
                            //Need a new rect here
                            var x1 = x;
                            var y1 = y;
                            actions.Add(() =>
                                        {
                                            var rec = new Rectangle();
                                            Canvas.SetTop(rec,
                                                          y1 * PixelSize);
                                            Canvas.SetLeft(rec,
                                                           x1 * PixelSize);
                                            rec.Width = PixelSize;
                                            rec.Height = PixelSize;
                                            rec.Fill = new SolidColorBrush(Colors.Black);
                                            MainCanvas.Children.Add(rec);
                                            displayDictionary[point] = rec;
                                        });
                        }
                        else if (currentRect != null
                                 && !pixels[x,
                                            y])
                        {
                            //Need to remove the rect
                            actions.Add(() =>
                                        {
                                            MainCanvas.Children.Remove(currentRect);
                                            displayDictionary[point] = null;
                                        });
                        }
                    }
                }

                await Application.Current.Dispatcher.InvokeAsync(() => actions.ForEach(x => x()));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception {ex}",
                                "Exception");
            }
            finally
            {
                displayLock.Release();
            }
        }

        private readonly Dictionary<Point, Rectangle> displayDictionary;

        public MainWindow()
        {
            displayDictionary = new Dictionary<Point, Rectangle>();

            for(var x = 0; x < 64; x++)
            {
                for(var y = 0; y < 32; y++)
                {
                    var point = new Point(x,
                                          y);

                    displayDictionary[point] = null;
                }
            }

            var registers = new RegisterModule();

            emulator = CHIP8Factory.GetChip8(DisplayEmulatorScreen,
                                             registers,
                                             new StackModule(),
                                             new MemoryModule(Enumerable.Repeat<byte>(0x0,
                                                                                      4096)));

            emulator.ToneOn += this.ToneOn;
            emulator.ToneOff += this.ToneOff;

            var bytes = File.ReadAllBytes("pong.ch8");

            emulator.LoadProgram(bytes);

            InitializeComponent();

            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
        }

        private CancellationTokenSource toneToken;

        private void ToneOn(object sender,
                            EventArgs eventArgs)
        {
            toneToken = new CancellationTokenSource();
            Task.Run(() => ToneTask());
        }

        private void ToneTask()
        {
            while(!toneToken.IsCancellationRequested)
            {
                Console.Beep(440,
                             1_000);

                Thread.Sleep(900);
            }
        }

        private void ToneOff(object sender, EventArgs eventArgs)
        {
            toneToken.Cancel();
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
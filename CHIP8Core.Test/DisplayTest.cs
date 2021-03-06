﻿using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace CHIP8Core.Test
{
    public class DisplayTest
    {
        #region Instance Methods

        [Fact]
        public void Correctly_Clears_Display_CLS()
        {
            var registers = new RegisterModule();

            bool[,] display = null;

            Task WriteDisplay(bool[,] values)
            {
                display = values;
                return Task.CompletedTask;
            }

            var emulator = CHIP8Factory.GetChip8(WriteDisplay,
                                                 registers);

            registers.SetGeneralValue(0,
                                      0x0);

            var instructions = new byte[]
                               {
                                   /* Load location of sprite for '0' to I */
                                   0xF0,
                                   0x29,
                                   /* Draw 5 byte sized sprite at 0,0 */
                                   0xD0,
                                   0x05,
                                   /* Clear screen */
                                   0x00,
                                   0xE0
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            Assert.NotNull(display);

            for (var x = 0; x < display.GetLength(0); x++)
            {
                for (var y = 0; y < display.GetLength(1); y++)
                {
                    Assert.False(display[x,
                                         y]);
                }
            }
        }

        [Fact]
        public void Correctly_Draws_Manual_Zero_Sprite_Dxyn()
        {
            /*
             * Dxyn - DRW Vx, Vy, nibble
                Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
             */

            // Copy of the 0 sprite data we load
            var zeroSpriteData = new byte[]
                                 {
                                     0xF0,
                                     0x90,
                                     0x90,
                                     0x90,
                                     0xF0
                                 };

            // Set I to start of sprite
            var setIInstruction = new byte[]
                                  {
                                      0xAD,
                                      0xDD
                                  };

            // Draw 5 bytes at 0,0 from mem address I
            var drawInstruction = new byte[]
                                  {
                                      0xD0,
                                      0x05
                                  };

            var registers = new RegisterModule();

            bool[,] display = null;

            Task WriteDisplay(bool[,] values)
            {
                display = values;
                return Task.CompletedTask;
            }

            var ram = Enumerable.Repeat<byte>(0x0,
                                              4096)
                                .ToArray();

            const int spriteStart = 0xDDD;

            // Copy the data starting at FFF
            for (var i = spriteStart; i < spriteStart + zeroSpriteData.Length; i++)
            {
                ram[i] = zeroSpriteData[i - spriteStart];
            }

            var memory = new MemoryModule(ram);

            var emulator = CHIP8Factory.GetChip8(WriteDisplay,
                                                 registers,
                                                 new StackModule(),
                                                 memory);

            registers.SetGeneralValue(0,
                                      0x0);

            var instructions = setIInstruction.Concat(drawInstruction)
                                              .ToArray();

            emulator.LoadProgram(instructions);

            emulator.Start();

            void AssertDisplay(int x,
                               int y,
                               bool pixelValue)
            {
                var validY = false;

                switch (x)
                {
                    case 0:
                    case 3:
                        // Column 1 & 4 all on
                        validY = pixelValue;
                        break;
                    case 1:
                    case 2:
                        // For the middle 2 columns only the top and bottom should be on
                        if (y == 0
                            || y == 4)
                        {
                            validY = pixelValue;
                        }
                        else
                        {
                            validY = !pixelValue;
                        }

                        break;
                    default:
                        // Everything past 4 should be off
                        validY = !pixelValue;
                        break;
                }

                Assert.True(validY,
                            $"Invalid pixel at {x},{y}. {pixelValue}.");
            }

            Assert.NotNull(display);

            // Check the 8x5 area the sprite displays in
            for (var x = 0; x < 8; x++)
            {
                for (var y = 0; y < 5; y++)
                {
                    AssertDisplay(x,
                                  y,
                                  display[x,
                                          y]);
                }
            }
        }

        [Theory]
        [InlineData(0,
                    0)]
        [InlineData(10,
                    15)]
        public void Correctly_Draws_Zero_Sprite_Dxyn(byte xOffset,
                                                     byte yOffset)
        {
            var registers = new RegisterModule();

            bool[,] display = null;

            Task WriteDisplay(bool[,] values)
            {
                display = values;
                return Task.CompletedTask;
            }

            var emulator = CHIP8Factory.GetChip8(WriteDisplay,
                                                 registers);

            registers.SetGeneralValue(3,
                                      0x0);

            registers.SetGeneralValue(0,
                                      xOffset);
            registers.SetGeneralValue(1,
                                      yOffset);

            var instructions = new byte[]
                               {
                                   /* Load location of sprite for '0' to I */
                                   0xF3,
                                   0x29,
                                   /* Draw 5 byte sized sprite at (v0, v1) */
                                   0xD0,
                                   0x15
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            void AssertDisplay(int x,
                               int y,
                               bool pixelValue)
            {
                var validY = false;

                x = x - xOffset;
                y = y - yOffset;

                switch (x)
                {
                    case 0:
                    case 3:
                        // Column 1 & 4 all on
                        validY = pixelValue;
                        break;
                    case 1:
                    case 2:
                        // For the middle 2 columns only the top and bottom should be on
                        if (y == 0
                            || y == 4)
                        {
                            validY = pixelValue;
                        }
                        else
                        {
                            validY = !pixelValue;
                        }

                        break;
                    default:
                        // Everything past 4 should be off
                        validY = !pixelValue;
                        break;
                }

                Assert.True(validY,
                            $"Invalid pixel at sprite coordinate ({x},{y}), true coordinate({x + xOffset}, {y + yOffset}). {pixelValue}.");
            }

            Assert.NotNull(display);

            // Check the 8x5 area the sprite displays in
            for (var x = xOffset; x < 8 + xOffset; x++)
            {
                for (var y = yOffset; y < 5 + yOffset; y++)
                {
                    AssertDisplay(x,
                                  y,
                                  display[x,
                                          y]);
                }
            }
        }

        #endregion
    }
}
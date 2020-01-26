using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Xunit;

namespace CHIP8Core.Test
{
    public class ArithmeticTest
    {
        [Fact]
        public void LD_vx_vy()
        {
            /*
             * 8xy0 - LD Vx, Vy Set Vx = Vy. Stores the value of register Vy in register Vx.
             */

            var emulator = new CHIP8(null);

            var instructions = new byte[]
                               {
                                   0x61, //LD v1 with 0xAC
                                   0xAC,
                                   0x80, //LD v0 with v1
                                   0x10
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var registers = GetRegisters(emulator);

            Assert.Equal(0xAC,
                         registers[0]);
        }

        public byte[] GetRegisters(CHIP8 emulator)
        {
            //TODO need to break registers & ram into their own classes
            return (byte[])typeof(CHIP8).GetField("generalRegisters",
                                                  BindingFlags.Instance | BindingFlags.NonPublic)
                                        .GetValue(emulator);
        }
    }
}

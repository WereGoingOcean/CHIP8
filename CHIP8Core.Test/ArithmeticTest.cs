using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Moq;

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
            var registers = new RegisterModule();

            var emulator = new CHIP8(null,
                                     registers);

            registers.SetGeneralValue(1,
                                      0xAC);

            var instructions = new byte[]
                               {
                                   0x80, //LD v0 with v1
                                   0x10
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            Assert.Equal(0xAC,
                         registers.GetGeneralValue(0));
        }
    }
}

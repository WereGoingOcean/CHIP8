using System.Linq;

using Xunit;

namespace CHIP8Core.Test
{
    public class BitwiseTests
    {
        [Theory]
        [InlineData(0x12,
                    0x24)]
        [InlineData(0x05,
                    0x10)]
        public void _8xy1_OR_vx_vy(byte x,
                                   byte y)
        {
            /*
             * 8xy1 - Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx.
             */
            var registers = new RegisterModule();

            var emulator = CHIP8Factory.GetChip8(registers: registers);

            var instructions = new byte[]
                               {
                                   0x60, //LD v0 with x,
                                   x,
                                   0x61, //LD v1 with y
                                   y,
                                   0x80, //Bitwise or and store in v0
                                   0x11
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var expectedResult = x | y;

            Assert.Equal(expectedResult,
                         registers.GetGeneralValue(0));
        }

        [Theory]
        [InlineData(0x12,
                    0x24)]
        [InlineData(0x05,
                    0x10)]
        public void _8xy2_AND_vx_vy(byte x,
                                   byte y)
        {
            /*
             * 8xy2 - Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx.
             */
            var registers = new RegisterModule();

            var emulator = CHIP8Factory.GetChip8(registers: registers);

            var instructions = new byte[]
                               {
                                   0x60, //LD v0 with x,
                                   x,
                                   0x61, //LD v1 with y
                                   y,
                                   0x80, //Bitwise and, store in v0
                                   0x12
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var expectedResult = x & y;

            Assert.Equal(expectedResult,
                         registers.GetGeneralValue(0));
        }

        [Theory]
        [InlineData(0x12,
                    0x24)]
        [InlineData(0x05,
                    0x10)]
        public void _8xy3_XOR_vx_vy(byte x,
                                    byte y)
        {
            /*
             * 8xy3 - Performs a bitwise exclusive OR on the values of Vx and Vy, then stores the result in Vx.
             */
            var registers = new RegisterModule();

            var emulator = CHIP8Factory.GetChip8(registers: registers);

            var instructions = new byte[]
                               {
                                   0x60, //LD v0 with x,
                                   x,
                                   0x61, //LD v1 with y
                                   y,
                                   0x80, //Bitwise xor, store in v0
                                   0x13
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var expectedResult = x ^ y;

            Assert.Equal(expectedResult,
                         registers.GetGeneralValue(0));
        }
    }
}
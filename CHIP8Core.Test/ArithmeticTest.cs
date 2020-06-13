using System.Collections;
using System.Linq;

using Xunit;

namespace CHIP8Core.Test
{
    public class ArithmeticTest
    {
        #region Instance Methods

        [Fact]
        public void _7xkk_ADD_vx_byte()
        {
            /*
             * 7xkk - ADD Vx, byte Set Vx = Vx + kk. Adds the value kk to the value of register Vx, then stores the result in Vx.
             */
            var registers = new RegisterModule();

            var emulator = GetChip(registers);

            // Set v0 to 0x10
            registers.SetGeneralValue(0,
                                      0x10);

            // Add 0x12 to v0
            var instructions = new byte[]
                               {
                                   0x70,
                                   0x12
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            Assert.Equal(0x22,
                         registers.GetGeneralValue(0));
        }

        [Fact]
        public void _8xy0_LD_vx_vy()
        {
            /*
             * 8xy0 - LD Vx, Vy Set Vx = Vy. Stores the value of register Vy in register Vx.
             */
            var registers = new RegisterModule();

            var emulator = GetChip(registers);

            // Set v1 to AC
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

        [Theory]
        [InlineData(0x1,
                    0x0)]
        [InlineData(0x0,
                    0x1)]
        [InlineData(0x0,
                    0x0)]
        public void _8xy1_OR_vx_vy(byte v0,
                                   byte v1)
        {
            /*
             * 
8xy1 - OR Vx, Vy
Set Vx = Vx OR Vy.

Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx. A bitwise OR compares the corresponding bits from two values, and if either bit is 1, then the same bit in the result is also 1. Otherwise, it is 0. 
             */
            var registers = new RegisterModule();

            var emulator = GetChip(registers);

            registers.SetGeneralValue(0,
                                      v0);
            registers.SetGeneralValue(1,
                                      v1);

            // Set v0 = v0 || v1
            var instructions = new byte[]
                               {
                                   0x80,
                                   0x11
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var expectedResult = v0 | v1;

            Assert.Equal(expectedResult,
                         registers.GetGeneralValue(0));
        }

        [Theory]
        [InlineData(0x1,
                    0x0)]
        [InlineData(0x0,
                    0x1)]
        [InlineData(0x0,
                    0x0)]
        public void _8xy2_AND_vx_vy(byte v0,
                                    byte v1)
        {
            /*
             * 
8xy2 - AND Vx, Vy
Set Vx = Vx AND Vy.

Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx. A bitwise AND compares the corresponding bits from two values, and if both bits are 1, then the same bit in the result is also 1. Otherwise, it is 0. 
             */
            var registers = new RegisterModule();

            var emulator = GetChip(registers);

            registers.SetGeneralValue(0,
                                      v0);
            registers.SetGeneralValue(1,
                                      v1);

            var instructions = new byte[]
                               {
                                   0x80,
                                   0x12
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var expectedResult = v0 & v1;

            Assert.Equal(expectedResult,
                         registers.GetGeneralValue(0));
        }

        [Theory]
        [InlineData(0x1,
                    0x0)]
        [InlineData(0x0,
                    0x1)]
        [InlineData(0x0,
                    0x0)]
        public void _8xy3_XOR_vx_vy(byte v0,
                                    byte v1)
        {
            /*
             * 
8xy3 - XOR Vx, Vy
Set Vx = Vx XOR Vy.

Performs a bitwise exclusive OR on the values of Vx and Vy, then stores the result in Vx. An exclusive OR compares the corresponding bits from two values, and if the bits are not both the same, then the corresponding bit in the result is set to 1. Otherwise, it is 0. 
             */
            var registers = new RegisterModule();

            var emulator = GetChip(registers);

            registers.SetGeneralValue(0,
                                      v0);
            registers.SetGeneralValue(1,
                                      v1);

            var instructions = new byte[]
                               {
                                   0x80,
                                   0x13
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var expectedResult = v0 ^ v1;

            Assert.Equal(expectedResult,
                         registers.GetGeneralValue(0));
        }

        [Theory]
        [InlineData(0x12,
                    0xAC)]
        [InlineData(0xAA,
                    0xAA)]
        [InlineData(0x12,
                    0x3)]
        public void _8xy4_ADD_vx_vy(byte v0,
                                    byte v1)
        {
            /*
             * 
8xy4 - ADD Vx, Vy
Set Vx = Vx + Vy, set VF = carry.

The values of Vx and Vy are added together. If the result is greater than 8 bits (i.e., > 255,) VF is set to 1, otherwise 0. Only the lowest 8 bits of the result are kept, and stored in Vx.

             */
            var registers = new RegisterModule();

            var emulator = GetChip(registers);

            registers.SetGeneralValue(0,
                                      v0);
            registers.SetGeneralValue(1,
                                      v1);

            var instructions = new byte[]
                               {
                                   0x80,
                                   0x14
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var expectedResult = (v0 + v1) & 0xFF; // Only lowest 8bits is kept in the case we're > 255

            Assert.Equal(expectedResult,
                         registers.GetGeneralValue(0));

            var expectedFlag = v0 + v1 > 0xFF
                                   ? 0x1
                                   : 0x0;

            Assert.Equal(expectedFlag,
                         registers.GetGeneralValue(0xF));
        }

        [Theory]
        [InlineData(0x12,
                    0xAC)]
        [InlineData(0xAA,
                    0xAA)]
        [InlineData(0x12,
                    0x3)]
        public void _8xy5_SUB_vx_vy(byte v0,
                                    byte v1)
        {
            /*
8xy5 - SUB Vx, Vy
Set Vx = Vx - Vy, set VF = NOT borrow.

If Vx > Vy, then VF is set to 1, otherwise 0. Then Vy is subtracted from Vx, and the results stored in Vx.
             */

            var registers = new RegisterModule();

            var emulator = GetChip(registers);

            registers.SetGeneralValue(0,
                                      v0);
            registers.SetGeneralValue(1,
                                      v1);

            var instructions = new byte[]
                               {
                                   0x80,
                                   0x15
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var expectedResult = (v0 - v1) & 0xFF; // Only lowest 8bits is kept in the case we're > 255

            Assert.Equal(expectedResult,
                         registers.GetGeneralValue(0));

            var expectedFlag = v0 > v1
                                   ? 0x1
                                   : 0x0;

            Assert.Equal(expectedFlag,
                         registers.GetGeneralValue(0xF));
        }

        [Theory]
        [InlineData(0x12,
                    0xAC)]
        [InlineData(0xAA,
                    0xAA)]
        [InlineData(0x12,
                    0x3)]
        public void _8xy6_SHR_vx_vy(byte v0,
                                    byte v1)
        {
            /*
8xy6 - SHR Vx {, Vy}
Set Vx = Vx SHR 1.

If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0. Then Vx is divided by 2.
             */
            var registers = new RegisterModule();

            var emulator = GetChip(registers);

            registers.SetGeneralValue(0,
                                      v0);
            registers.SetGeneralValue(1,
                                      v1);

            var instructions = new byte[]
                               {
                                   0x80,
                                   0x16
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var expectedResult = (v0 / 2) & 0xFF; // Only lowest 8bits is kept in the case we're > 255

            Assert.Equal(expectedResult,
                         registers.GetGeneralValue(0));

            var expectedFlag = v0 & 0x1;

            Assert.Equal(expectedFlag,
                         registers.GetGeneralValue(0xF));
        }

        [Theory]
        [InlineData(0x12,
                    0xAC)]
        [InlineData(0xAA,
                    0xAA)]
        [InlineData(0x12,
                    0x3)]
        public void _8xy7_SUBN_vx_vy(byte v0,
                                     byte v1)
        {
            /*
8xy7 - SUBN Vx, Vy
Set Vx = Vy - Vx, set VF = NOT borrow.

If Vy > Vx, then VF is set to 1, otherwise 0. Then Vx is subtracted from Vy, and the results stored in Vx.             */
            var registers = new RegisterModule();

            var emulator = GetChip(registers);

            registers.SetGeneralValue(0,
                                      v0);
            registers.SetGeneralValue(1,
                                      v1);

            var instructions = new byte[]
                               {
                                   0x80,
                                   0x17
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var expectedResult = (v1 - v0) & 0xFF; // Only lowest 8bits is kept in the case we're > 255

            Assert.Equal(expectedResult,
                         registers.GetGeneralValue(0));

            var expectedFlag = v1 > v0
                                   ? 0x1
                                   : 0x0;

            Assert.Equal(expectedFlag,
                         registers.GetGeneralValue(0xF));
        }

        [Theory]
        [InlineData(0x12,
                    0xAC)]
        [InlineData(0xAA,
                    0xAA)]
        [InlineData(0x12,
                    0x3)]
        public void _8xyE_SHL_vx_vy(byte v0,
                                    byte v1)
        {
            /*
8xyE - SHL Vx {, Vy}
Set Vx = Vx SHL 1.

If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2.
             */
            var registers = new RegisterModule();

            var emulator = GetChip(registers);

            registers.SetGeneralValue(0,
                                      v0);
            registers.SetGeneralValue(1,
                                      v1);

            var instructions = new byte[]
                               {
                                   0x80,
                                   0x1E
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            var expectedResult = (v0 * 2) & 0xFF; // Only lowest 8bits is kept in the case we're > 255

            Assert.Equal(expectedResult,
                         registers.GetGeneralValue(0));

            var expectedFlag = new BitArray(new[]
                                            {
                                                v0
                                            }).Get(7)
                                   ? 0x1
                                   : 0x0;

            Assert.Equal(expectedFlag,
                         registers.GetGeneralValue(0xF));
        }

        private CHIP8 GetChip(IRegisterModule registers)
        {
            return new CHIP8(null,
                             registers,
                             new StackModule(),
                             new MemoryModule(Enumerable.Repeat<byte>(0x0,
                                                                      4096)));
        }

        #endregion
    }
}
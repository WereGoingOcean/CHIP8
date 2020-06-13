using System.Linq;

using Xunit;

namespace CHIP8Core.Test
{
    public class MemoryTest
    {
        #region Instance Methods

        [Fact]
        public void _6xkk_LD_vx_byte()
        {
            /*
             * 6xkk - LD Vx, byte Set Vx = kk. The interpreter puts the value kk into register Vx.
             */
            var registers = new RegisterModule();

            var emulator = new CHIP8(null,
                                     registers,
                                     new StackModule(),
                                     new MemoryModule(Enumerable.Repeat<byte>(0x0,
                                                                              4096)));

            var instructions = new byte[]
                               {
                                   0x60, //LD v0 with byte 0x12
                                   0x12
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            Assert.Equal(0x12,
                         registers.GetGeneralValue(0));
        }

        [Fact]
        public void _7xkk_LD_vx_byte()
        {
            /*
             * 7xkk - Adds the value kk to the value of register Vx, then stores the result in Vx. 
             */
            var registers = new RegisterModule();

            var emulator = new CHIP8(null,
                                     registers,
                                     new StackModule(),
                                     new MemoryModule(Enumerable.Repeat<byte>(0x0,
                                                                              4096)));

            var instructions = new byte[]
                               {
                                   0x60, //LD v0 with byte 0x12
                                   0x12,
                                   0x70, // Add 0x10 to v0 and save it in v0
                                   0x10
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
             * 8xy0 - Stores the value of register Vy in register Vx.
             */
            var registers = new RegisterModule();

            var emulator = new CHIP8(null,
                                     registers,
                                     new StackModule(),
                                     new MemoryModule(Enumerable.Repeat<byte>(0x0,
                                                                              4096)));

            var instructions = new byte[]
                               {
                                   0x61, //LD v1 with byte 0x12
                                   0x12,
                                   0x80, // Store value at v1 in v0
                                   0x10
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            Assert.Equal(0x12,
                         registers.GetGeneralValue(0));
        }

        #endregion
    }
}
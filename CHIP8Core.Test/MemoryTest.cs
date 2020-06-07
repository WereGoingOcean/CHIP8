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
                                     new StackModule());

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

        #endregion
    }
}
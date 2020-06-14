using System.Linq;
using System.Reflection;

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

            var emulator = CHIP8Factory.GetChip8(registers: registers);

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

            var emulator = CHIP8Factory.GetChip8(registers: registers);

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

            var emulator = CHIP8Factory.GetChip8(registers: registers);

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

        [Fact]
        public void _Annn_LD_I_addr()
        {
            var registers = new RegisterModule();

            var emulator = CHIP8Factory.GetChip8(registers: registers);

            var instructions = new byte[]
                               {
                                   0xA1,
                                   0x23
                               };

            emulator.LoadProgram(instructions);

            emulator.Start();

            Assert.Equal(0x123,
                         registers.GetI());
        }

        [Fact]
        public void _Bnnn_JP_v0_addr()
        {
            var registers = new RegisterModule();

            registers.SetGeneralValue(0,
                                      0x12);

            var emulator = CHIP8Factory.GetChip8(registers: registers);

            var instructions = new byte[]
                               {
                                   0xB1,
                                   0x23
                               };

            emulator.LoadProgram(instructions);

            emulator.Tick += (e,
                              a) =>
                             {
                                 emulator.Stop();
                             };

            emulator.Start();

            Assert.Equal(0x123 + 0x12,
                         GetProgramCounter(emulator));
        }

        [Theory]
        [InlineData(0x12,
                    0x20)]
        [InlineData(0x05,
                    0xA1)]
        public void _Cxkk_RND_vx_byte(byte kk,
                                      byte rand)
        {
            var registers = new RegisterModule();

            var random = new TestRandom(rand);


            var emulator = CHIP8Factory.GetChip8(registers: registers,
                                                 random: random);

            var instructions = new byte[]
                               {
                                   0xC0, //Set v0 = kk & random byte
                                   kk
                               };

            emulator.LoadProgram(instructions);

            emulator.Tick += (e,
                              a) =>
                             {
                                 emulator.Stop();
                             };

            emulator.Start();

            Assert.Equal(rand & kk,
                         registers.GetGeneralValue(0));
        }

        private ushort GetProgramCounter(CHIP8 chip)
        {
            return (ushort)typeof(CHIP8).GetField("programCounter",
                                                  BindingFlags.Instance | BindingFlags.NonPublic)
                                        .GetValue(chip);
        }

        #endregion

        class TestRandom : IRandomModule
        {
            private readonly byte randomByte;

            public TestRandom(byte randomByte)
            {
                this.randomByte = randomByte;
            }

            public byte GetNextRandom()
            {
                return randomByte;
            }
        }
    }
}
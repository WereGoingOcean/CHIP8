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

        [Fact]
        public void _Fx1E_add_I_Vx()
        {
            var registers = new RegisterModule();

            registers.SetGeneralValue(0,
                                      0x12);

            registers.SetI(0x123);

            var emulator = CHIP8Factory.GetChip8(registers: registers);

            var instructions = new byte[]
                               {
                                   0xF0, //Add v0 and I, store in I
                                   0x1E
                               };

            emulator.LoadProgram(instructions);

            emulator.Tick += (e,
                              a) =>
                             {
                                 emulator.Stop();
                             };

            emulator.Start();

            Assert.Equal(0x123 + 0x12,
                         registers.GetI());
        }

        [Fact]
        public void _Fx33_LD_B_Vx()
        {
            /*
             * Stores the BCD of vx in I, I+1, and I+2 of memory
             * 125:
             * Hex: 0x7D
             * BCD: 0001 0010 0101
             */

            var registers = new RegisterModule();

            registers.SetGeneralValue(0,
                                      0x7D);

            registers.SetI(0x123);

            var memory = new MemoryModule(Enumerable.Repeat((byte)0x0,
                                                            4096));

            var emulator = CHIP8Factory.GetChip8(registers: registers,
                                                 mem: memory);

            var instructions = new byte[]
                               {
                                   0xF0, //Store BCD of v0 in memory
                                   0x33
                               };

            emulator.LoadProgram(instructions);

            emulator.Tick += (e,
                              a) =>
                             {
                                 emulator.Stop();
                             };

            emulator.Start();

            var startPoint = registers.GetI();

            var hundreds = memory[startPoint];

            startPoint++;

            var tens = memory[startPoint];

            startPoint++;

            var ones = memory[startPoint];

            Assert.Equal(0x1,
                         hundreds);

            Assert.Equal(0x2,
                         tens);

            Assert.Equal(0x5,
                         ones);
        }

        [Fact]
        public void _Fx55_LD_I_Vx()
        {
            /*
             * Stores registers v0 through vx in memory starting at I
             */

            var registers = new RegisterModule();

            registers.SetGeneralValue(0,
                                      0x7D);

            registers.SetGeneralValue(1,
                                      0x55);

            registers.SetGeneralValue(2,
                                      0x18);

            registers.SetGeneralValue(3,
                                      0x90);

            registers.SetI(0x278);

            var memory = new MemoryModule(Enumerable.Repeat((byte)0x0,
                                                            4096));

            var emulator = CHIP8Factory.GetChip8(registers: registers,
                                                 mem: memory);

            var instructions = new byte[]
                               {
                                   0xF3, //Store v0 - v3 in memory
                                   0x55
                               };

            emulator.LoadProgram(instructions);

            emulator.Tick += (e,
                              a) =>
                             {
                                 emulator.Stop();
                             };

            emulator.Start();

            var startPoint = 0x278;

            var v0 = memory[startPoint];

            startPoint++;

            var v1 = memory[startPoint];

            startPoint++;

            var v2 = memory[startPoint];

            startPoint++;

            var v3 = memory[startPoint];

            Assert.Equal(0x7D,
                         v0);
            Assert.Equal(0x55,
                         v1);
            Assert.Equal(0x18,
                         v2);
            Assert.Equal(0x90,
                         v3);
        }

        [Fact]
        public void _Fx65_LD_Vx_I()
        {
            /*
             * Set registers v0 through vx from memory starting at I
             */

            var registers = new RegisterModule();
            
            registers.SetI(0x278);

            var startPoint = registers.GetI();

            var memory = new MemoryModule(Enumerable.Repeat((byte)0x0,
                                                            4096));

            memory[startPoint] = 0x7D;

            startPoint++;

            memory[startPoint] = 0x55;

            startPoint++;

            memory[startPoint] = 0x18;

            startPoint++;

            memory[startPoint] = 0x90;

            var emulator = CHIP8Factory.GetChip8(registers: registers,
                                                 mem: memory);

            var instructions = new byte[]
                               {
                                   0xF3, //Set v0 - v3 from memory
                                   0x65
                               };

            emulator.LoadProgram(instructions);

            emulator.Tick += (e,
                              a) =>
                             {
                                 emulator.Stop();
                             };

            emulator.Start();

            var v0 = registers.GetGeneralValue(0);

            var v1 = registers.GetGeneralValue(1);

            var v2 = registers.GetGeneralValue(2);

            var v3 = registers.GetGeneralValue(3);

            Assert.Equal(0x7D,
                         v0);
            Assert.Equal(0x55,
                         v1);
            Assert.Equal(0x18,
                         v2);
            Assert.Equal(0x90,
                         v3);
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
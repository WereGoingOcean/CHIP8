using System.Reflection;

using Xunit;

namespace CHIP8Core.Test
{
    public class ControlFlowTest
    {
        #region Instance Methods

        [Theory]
        [InlineData(600)]
        public void _00EE_RET(ushort addr)
        {
            // Make sure it's a valid address and doesn't cause an infinite loop
            Assert.InRange(addr,
                           514,
                           4094);

            var stackModule = new StackModule();

            stackModule.Push(addr);

            var instructions = new byte[]
                               {
                                   0x00,
                                   0xEE
                               };

            var chip = CHIP8Factory.GetChip8(stack: stackModule);

            chip.LoadProgram(instructions);

            chip.Tick += (c,
                          e) =>
                         {
                             chip.Stop();
                         };

            chip.Start();

            var programCounter = GetProgramCounter(chip);

            Assert.Equal(addr,
                         programCounter);
        }

        [Fact]
        public void _1nnn_JP()
        {
            var instructions = new byte[]
                               {
                                   0x11,
                                   0xEF
                               };

            var expectedAddress = (ushort)(0x11EF & 0x0FFF);

            var chip = CHIP8Factory.GetChip8();

            chip.LoadProgram(instructions);

            chip.Tick += (c,
                          e) =>
                         {
                             chip.Stop();
                         };

            chip.Start();

            var programCounter = GetProgramCounter(chip);

            Assert.Equal(expectedAddress,
                         programCounter);
        }

        [Fact]
        public void _2nnn_CALL()
        {
            var instructions = new byte[]
                               {
                                   0x21,
                                   0xEF
                               };

            var expectedAddress = (ushort)(0x21EF & 0x0FFF);

            var stackModule = new StackModule();

            var chip = CHIP8Factory.GetChip8(stack: stackModule);

            chip.LoadProgram(instructions);

            chip.Tick += (c,
                          e) =>
                         {
                             chip.Stop();
                         };

            chip.Start();

            var programCounter = GetProgramCounter(chip);

            Assert.Equal(expectedAddress,
                         programCounter);

            var itemOnStack = stackModule.Pop();

            // Stack should have the program counter which has only moved up one past the start at 512
            Assert.Equal(514,
                         itemOnStack);
        }

        [Theory]
        [InlineData(0x12,
                    0x10)]
        [InlineData(0x20,
                    0x20)]
        public void _3xkk_SE_vx_byte(byte vx,
                                     byte kk)
        {
            var registerModule = new RegisterModule();

            registerModule.SetGeneralValue(0,
                                           vx);

            var instructions = new byte[]
                               {
                                   0x30, //Inc program counter by 2 if v0 == kk
                                   kk
                               };

            var chip = CHIP8Factory.GetChip8(registers: registerModule);

            chip.LoadProgram(instructions);

            chip.Tick += (c,
                          e) =>
                         {
                             chip.Stop();
                         };

            chip.Start();

            var nextInstruction = 514;

            var expectedAddress = nextInstruction;

            if (vx == kk)
            {
                expectedAddress += 2;
            }

            var programCounter = GetProgramCounter(chip);

            Assert.Equal(expectedAddress,
                         programCounter);
        }

        [Theory]
        [InlineData(0x12,
                    0x10)]
        [InlineData(0x20,
                    0x20)]
        public void _4xkk_SNE_vx_byte(byte vx,
                                      byte kk)
        {
            var registerModule = new RegisterModule();

            registerModule.SetGeneralValue(0,
                                           vx);

            var instructions = new byte[]
                               {
                                   0x40, //Inc program counter by 2 if v0 != kk
                                   kk
                               };

            var chip = CHIP8Factory.GetChip8(registers: registerModule);

            chip.LoadProgram(instructions);

            chip.Tick += (c,
                          e) =>
                         {
                             chip.Stop();
                         };

            chip.Start();

            var nextInstruction = 514;

            var expectedAddress = nextInstruction;

            if (vx != kk)
            {
                expectedAddress += 2;
            }

            var programCounter = GetProgramCounter(chip);

            Assert.Equal(expectedAddress,
                         programCounter);
        }

        [Theory]
        [InlineData(0x12,
                    0x10)]
        [InlineData(0x20,
                    0x20)]
        public void _5xy0_SE_vx_vy(byte vx,
                                   byte vy)
        {
            var registerModule = new RegisterModule();

            registerModule.SetGeneralValue(0,
                                           vx);

            registerModule.SetGeneralValue(1,
                                           vy);

            var instructions = new byte[]
                               {
                                   0x50, //Inc program counter by 2 if v0 == v1
                                   0x10
                               };

            var chip = CHIP8Factory.GetChip8(registers: registerModule);

            chip.LoadProgram(instructions);

            chip.Tick += (c,
                          e) =>
                         {
                             chip.Stop();
                         };

            chip.Start();

            var nextInstruction = 514;

            var expectedAddress = nextInstruction;

            if (vx == vy)
            {
                expectedAddress += 2;
            }

            var programCounter = GetProgramCounter(chip);

            Assert.Equal(expectedAddress,
                         programCounter);
        }

        [Theory]
        [InlineData(0x12,
                    0x10)]
        [InlineData(0x20,
                    0x20)]
        public void _9xy0_SNE_vx_vy(byte vx,
                                    byte vy)
        {
            var registerModule = new RegisterModule();

            registerModule.SetGeneralValue(0,
                                           vx);

            registerModule.SetGeneralValue(1,
                                           vy);

            var instructions = new byte[]
                               {
                                   0x90, //Inc program counter by 2 if v0 == v1
                                   0x10
                               };

            var chip = CHIP8Factory.GetChip8(registers: registerModule);

            chip.LoadProgram(instructions);

            chip.Tick += (c,
                          e) =>
                         {
                             chip.Stop();
                         };

            chip.Start();

            var nextInstruction = 514;

            var expectedAddress = nextInstruction;

            if (vx != vy)
            {
                expectedAddress += 2;
            }

            var programCounter = GetProgramCounter(chip);

            Assert.Equal(expectedAddress,
                         programCounter);
        }

        private ushort GetProgramCounter(CHIP8 chip)
        {
            return (ushort)typeof(CHIP8).GetField("programCounter",
                                                  BindingFlags.Instance | BindingFlags.NonPublic)
                                        .GetValue(chip);
        }

        #endregion
    }
}
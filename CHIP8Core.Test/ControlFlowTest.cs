using System.Linq;
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

            var registerModule = new RegisterModule();

            stackModule.Push(addr);

            var instructions = new byte[]
                               {
                                   0x00,
                                   0xEE
                               };

            var chip = new CHIP8(null,
                                 registerModule,
                                 stackModule,
                                 new MemoryModule(Enumerable.Repeat<byte>(0x0,
                                                                          4096)));

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
            var stackModule = new StackModule();

            var registerModule = new RegisterModule();

            var instructions = new byte[]
                               {
                                   0x11,
                                   0xEF
                               };

            var expectedAddress = (ushort)(0x11EF & 0x0FFF);

            var chip = new CHIP8(null,
                                 registerModule,
                                 stackModule,
                                 new MemoryModule(Enumerable.Repeat<byte>(0x0,
                                                                          4096)));

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
            var stackModule = new StackModule();

            var registerModule = new RegisterModule();

            var instructions = new byte[]
                               {
                                   0x21,
                                   0xEF
                               };

            var expectedAddress = (ushort)(0x21EF & 0x0FFF);

            var chip = new CHIP8(null,
                                 registerModule,
                                 stackModule,
                                 new MemoryModule(Enumerable.Repeat<byte>(0x0,
                                                                          4096)));

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

        private ushort GetProgramCounter(CHIP8 chip)
        {
            return (ushort)typeof(CHIP8).GetField("programCounter",
                                                  BindingFlags.Instance | BindingFlags.NonPublic)
                                        .GetValue(chip);
        }

        #endregion
    }
}
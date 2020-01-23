using Xunit;

namespace CHIP8Core.Test
{
    public class InstructionTest
    {
        #region Instance Methods

        [Theory]
        [InlineData(0xD123,
            0x23)]
        [InlineData(0xC778,
            0x78)]
        public void CorrectlyParsesKk(ushort instructionWord,
                                      byte kk)
        {
            var parsedInstruction = new Instruction(instructionWord);

            Assert.Equal(kk,
                         parsedInstruction.kk);
        }

        [Theory]
        [InlineData(0xD123,
            0x3)]
        [InlineData(0xC778,
            0x8)]
        public void CorrectlyParsesNibble(ushort instructionWord,
                                          byte nibble)
        {
            var parsedInstruction = new Instruction(instructionWord);

            Assert.Equal(nibble,
                         parsedInstruction.nibble);
        }

        [Theory]
        [InlineData(0x2ABC,
            0xABC)]
        [InlineData(0xA476,
            0x476)]
        public void CorrectlyParsesNNN(ushort instructionWord,
                                       ushort nnn)
        {
            var parsedInstruction = new Instruction(instructionWord);

            Assert.Equal(nnn,
                         parsedInstruction.addr);
        }

        [Theory]
        [InlineData(0xD123,
            0x1)]
        [InlineData(0xC798,
            0x7)]
        public void CorrectlyParsesX(ushort instructionWord,
                                     byte x)
        {
            var parsedInstruction = new Instruction(instructionWord);

            Assert.Equal(x,
                         parsedInstruction.x);
        }

        [Theory]
        [InlineData(0xD123,
            0x2)]
        [InlineData(0xC798,
            0x9)]
        public void CorrectlyParsesY(ushort instructionWord,
                                     byte y)
        {
            var parsedInstruction = new Instruction(instructionWord);

            Assert.Equal(y,
                         parsedInstruction.y);
        }

        #endregion
    }
}
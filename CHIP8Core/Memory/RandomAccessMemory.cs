using CHIP8Core.Models;

namespace CHIP8Core.Memory
{
    public class RandomAccessMemory
    {
        #region Constants

        private const int RamSizeInBytes = 4096;

        #endregion

        #region Fields

        private readonly byte[] memory = new byte[RamSizeInBytes];

        #endregion

        #region Instance Methods

        public byte ReadByte(TwoBytes address)
        {
            var memoryValue = memory[address];

            return memoryValue;
        }

        public TwoBytes ReadWord(TwoBytes address)
        {
            var index = (int)address;

            return new TwoBytes(memory[address],
                                memory[address + 1]);
        }

        public void WriteByte(TwoBytes address,
                              byte value)
        {
            memory[address] = value;
        }

        public void WriteWord(TwoBytes address,
                              TwoBytes value)
        {
            var index = (int)address;

            memory[index] = value.MostSignificant;
            memory[index + 1] = value.LeastSignificant;
        }

        #endregion
    }
}
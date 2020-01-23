namespace CHIP8Core
{
    public class Instruction
    {
        #region Constants

        private const ushort firstByteMask = 0xF000;

        private const ushort kkMask = 0x00FF;

        private const ushort nMask = 0x000F;

        private const ushort nnnMask = 0x0FFF;

        private const ushort xMask = 0x0F00;

        private const ushort yMask = 0x00F0;

        #endregion

        #region Constructors

        public Instruction(ushort instruction)
        {
            this.instruction = instruction;
        }

        #endregion

        #region Instance Properties

        public ushort addr
        {
            get
            {
                return (ushort)(instruction & nnnMask);
            }
        }

        /// <summary>
        /// Get a byte representing the first hex of the instruction.
        /// </summary>
        public byte FirstHex
        {
            get
            {
                var maskedWord = (ushort)(instruction & firstByteMask);

                return (byte)(maskedWord >> 12);
            }
        }

        public ushort instruction { get; }

        public byte kk
        {
            get
            {
                return (byte)(instruction & kkMask);
            }
        }

        public ushort nibble
        {
            get
            {
                //TODO maybe change this to byte
                return (ushort)(instruction & nMask);
            }
        }

        public byte x
        {
            get
            {
                var xWord = (ushort)(instruction & xMask);

                // Casting to byte removes upper bits, so first shift right
                return (byte)(xWord >> 8);
            }
        }

        public byte y
        {
            get
            {
                var yWord = (ushort)(instruction & yMask);

                return (byte)(yWord >> 4);
            }
        }

        #endregion

        #region Instance Methods

        public override string ToString()
        {
            return instruction.ToString("X4");
        }

        #endregion
    }
}
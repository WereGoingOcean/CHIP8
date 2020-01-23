namespace CHIP8Core
{
    public class Instruction
    {
        #region Constants

        private const ushort kkMask = 0x00FF;

        private const ushort nMask = 0x000F;

        private const ushort nnnMask = 0x0FFF;

        private const ushort xMask = 0x0F00;

        private const ushort yMask = 0x00F0;

        #endregion

        #region Fields

        private readonly ushort instruction;

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
    }
}
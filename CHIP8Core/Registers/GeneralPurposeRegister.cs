namespace CHIP8Core.Registers
{
    public class GeneralPurposeRegister : EightBitRegister
    {
        #region Fields

        public readonly int RegisterIndex;

        #endregion

        #region Constructors

        public GeneralPurposeRegister(int registerIndex)
        {
            RegisterIndex = registerIndex;
        }

        #endregion
    }
}
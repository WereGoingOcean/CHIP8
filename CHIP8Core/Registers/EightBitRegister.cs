namespace CHIP8Core.Registers
{
    public class EightBitRegister
    {
        #region Fields

        protected byte RegisterValue;

        #endregion

        #region Instance Methods

        public void SetRegister(byte newValue)
        {
            RegisterValue = newValue;
        }

        #endregion
    }
}
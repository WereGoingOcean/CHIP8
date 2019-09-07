namespace CHIP8Core.Registers
{
    public class SixteenBitRegister
    {
        #region Fields

        protected ushort registerValue;

        #endregion

        #region Instance Methods

        public void SetRegister(ushort newValue)
        {
            registerValue = newValue;
        }

        #endregion
    }
}
using CHIP8Core.Models;

namespace CHIP8Core.Registers
{
    public class SixteenBitRegister
    {
        #region Fields

        protected TwoBytes registerValue;

        #endregion

        #region Instance Methods

        protected void SetRegister(TwoBytes newValue)
        {
            registerValue = newValue;
        }

        #endregion
    }
}
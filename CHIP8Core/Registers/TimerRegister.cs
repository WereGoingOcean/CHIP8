namespace CHIP8Core.Registers
{
    public class TimerRegister : EightBitRegister
    {
        #region Instance Methods

        public void Decrement()
        {
            SetRegister(this.RegisterValue--);
        }

        #endregion
    }
}
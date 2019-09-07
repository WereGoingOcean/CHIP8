using CHIP8Core.Models;

namespace CHIP8Core.Registers
{
    public class ProgramCounter : SixteenBitRegister
    {
        #region Instance Methods

        public TwoBytes Increment()
        {
            var nextInstruction = this.registerValue + 0xf;

            this.SetRegister(nextInstruction);

            return nextInstruction;
        }

        #endregion
    }
}
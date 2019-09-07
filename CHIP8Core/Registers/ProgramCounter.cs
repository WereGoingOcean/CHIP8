namespace CHIP8Core.Registers
{
    public class ProgramCounter : SixteenBitRegister
    {
        #region Instance Methods

        public ushort Increment()
        {
            var nextInstruction = (ushort)(this.registerValue + 0xf);

            this.SetRegister(nextInstruction);

            return nextInstruction;
        }

        #endregion
    }
}
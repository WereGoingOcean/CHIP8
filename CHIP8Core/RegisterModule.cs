namespace CHIP8Core
{
    public interface IRegisterModule
    {
        #region Instance Methods

        byte GetGeneralValue(int index);

        ushort GetI();

        void SetGeneralValue(int index,
                             byte value);

        void SetI(ushort value);

        #endregion
    }

    public class RegisterModule : IRegisterModule
    {
        #region Fields

        /// <summary>
        /// 16 General Purpose Registers. V_f (register 16) is used as a flag and shouldn't be set by programs.
        /// </summary>
        private readonly byte[] generalRegisters = new byte[16];

        /// <summary>
        /// I register. Usually stores memory addresses, meaning typically only leftmost 12 bits are used.
        /// </summary>
        private ushort iRegister;

        #endregion

        #region Instance Methods

        public byte GetGeneralValue(int index)
        {
            return generalRegisters[index];
        }

        public ushort GetI()
        {
            return iRegister;
        }

        public void SetGeneralValue(int index,
                                    byte value)
        {
            generalRegisters[index] = value;
        }

        public void SetI(ushort value)
        {
            iRegister = value;
        }

        #endregion
    }
}
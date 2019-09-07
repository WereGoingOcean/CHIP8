using System.Linq;

namespace CHIP8Core.Registers
{
    public class RegisterModule
    {
        #region Fields

        private readonly ProgramCounter programCounter = new ProgramCounter();

        private TimerRegister delayTimer = new TimerRegister();

        private GeneralPurposeRegister[] generalPurposeRegisters;

        private SixteenBitRegister iRegister;

        private TimerRegister soundTimer = new TimerRegister();

        private EightBitRegister stackPointer = new EightBitRegister();

        #endregion

        #region Constructors

        public RegisterModule()
        {
            generalPurposeRegisters = Enumerable.Range(0,
                                                       16)
                                                .Select(x => new GeneralPurposeRegister(x))
                                                .ToArray();

            iRegister = new SixteenBitRegister();
        }

        #endregion

        #region Instance Methods

        public ushort IncrementProgramCounter()
        {
            return programCounter.Increment();
        }

        #endregion
    }
}
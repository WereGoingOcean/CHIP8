using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CHIP8Core
{
    public class CHIP8
    {
        #region Constants

        private const int MemorySize = 4096;

        #endregion

        #region Fields

        private readonly int[] ram = new int[MemorySize];

        private readonly Stack<ushort> stack = new Stack<ushort>();

        #endregion

        #region Nested type: EightBitRegister

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

        #endregion

        #region Nested type: GeneralPurposeRegister

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

            public void Start()
            {
                Task.Run(() => RunEmulator());
            }

            public void Stop()
            {
                
            }
            
            private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

            private const double ClockSpeedHertz = 500;

            private TimeSpan clockDelay = TimeSpan.FromSeconds(1.0 / ClockSpeedHertz);
            
            public async void RunEmulator()
            {
                do
                {
                    //TODO process
                    //Increment program counter
                    //Read instruction
                    //Process instruction
                    //Update display
                    //Check timer registers

                    await Task.Delay(clockDelay);
                }
                while (!tokenSource.Token.IsCancellationRequested);
            }

            #endregion
        }

        #endregion

        #region Nested type: Registers

        public class Registers
        {
            #region Fields

            private TimerRegister delayTimer = new TimerRegister();

            private GeneralPurposeRegister[] generalPurposeRegisters;

            private SixteenBitRegister iRegister;

            private SixteenBitRegister programCounter = new SixteenBitRegister();

            private TimerRegister soundTimer = new TimerRegister();

            private EightBitRegister stackPointer = new EightBitRegister();

            #endregion

            #region Constructors

            public Registers()
            {
                generalPurposeRegisters = Enumerable.Range(0,
                                                           16)
                                                    .Select(x => new GeneralPurposeRegister(x))
                                                    .ToArray();

                iRegister = new SixteenBitRegister();
            }

            #endregion
        }

        #endregion

        #region Nested type: SixteenBitRegister

        public class SixteenBitRegister
        {
            #region Fields

            private ushort registerValue;

            #endregion

            #region Instance Methods

            public void SetRegister(ushort newValue)
            {
                registerValue = newValue;
            }

            #endregion
        }

        #endregion

        #region Nested type: TimerRegister

        public class TimerRegister : EightBitRegister
        {
            #region Instance Methods

            public void Decrement()
            {
                SetRegister(this.RegisterValue--);
            }

            #endregion
        }

        #endregion
    }
}
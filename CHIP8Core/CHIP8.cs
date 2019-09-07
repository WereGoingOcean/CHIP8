using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using CHIP8Core.Registers;

namespace CHIP8Core
{
    public class CHIP8
    {
        #region Constants

        private const double ClockSpeedHertz = 500;

        private const int MemorySize = 4096;

        #endregion

        #region Fields

        private readonly TimeSpan clockDelay = TimeSpan.FromSeconds(1.0 / ClockSpeedHertz);

        private readonly int[] ram = new int[MemorySize];

        private readonly RegisterModule registerModule = new RegisterModule();

        private readonly Stack<ushort> stack = new Stack<ushort>();

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        #endregion

        #region Instance Methods

        public async void RunEmulator()
        {
            do
            {
                //TODO process
                //Increment program counter
                var nextInstruction = registerModule.IncrementProgramCounter();
                //Read instruction

                //Process instruction
                //Update display
                //Check timer registers

                await Task.Delay(clockDelay);
            }
            while (!tokenSource.Token.IsCancellationRequested);
        }

        public void Start()
        {
            Task.Run(() => RunEmulator());
        }

        public void Stop()
        {
        }

        #endregion
    }
}
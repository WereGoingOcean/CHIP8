using System;
using System.Linq;

namespace CHIP8Core
{
    public static class CHIP8Factory
    {
        #region Class Methods

        public static CHIP8 GetChip8(Action<bool[,]> writeDisplay = null,
                                     IRegisterModule registers = null,
                                     IStackModule stack = null,
                                     MemoryModule mem = null,
                                     IRandomModule random = null)
        {
            return new CHIP8(writeDisplay
                             ?? (x =>
                                 {
                                 }),
                             registers ?? new RegisterModule(),
                             stack ?? new StackModule(),
                             mem
                             ?? new MemoryModule(Enumerable.Repeat((byte)0x0,
                                                                   4096)),
                             random ?? new RandomModule());
        }

        #endregion
    }
}
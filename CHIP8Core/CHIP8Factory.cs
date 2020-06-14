using System;
using System.Linq;
using System.Threading.Tasks;

namespace CHIP8Core
{
    public static class CHIP8Factory
    {
        #region Class Methods

        public static CHIP8 GetChip8(Func<bool[,], Task> writeDisplay = null,
                                     IRegisterModule registers = null,
                                     IStackModule stack = null,
                                     MemoryModule mem = null,
                                     IRandomModule random = null)
        {
            return new CHIP8(writeDisplay ?? (x => Task.CompletedTask),
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
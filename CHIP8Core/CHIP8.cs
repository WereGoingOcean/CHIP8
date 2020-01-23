using System;
using System.Diagnostics;

namespace CHIP8Core
{
    public class CHIP8
    {
        #region Fields

        /// <summary>
        /// 16 bit register for program counter. Not accessible by programs. Typically starts at 512 (0x200). Some programs (ETI 600) start
        /// at 1536 (0x600). Not sure if the chip knows how to tell.
        /// </summary>
        private static ushort programCounter = 0x200;

        /// <summary>
        /// 16 General Purpose Registers. V_f (register 16) is used as a flag and shouldn't be set by programs.
        /// </summary>
        private readonly byte[] generalRegisters = new byte[16];

        /// <summary>
        /// I register. Usually stores memory addresses, meaning typically only leftmost 12 bits are used.
        /// </summary>
        private readonly ushort iRegister;

        private readonly byte[] ram = new byte[4096];

        /// <summary>
        /// Stack is an array of 16 16 bit values. Stores the address to return to when a sub routine finishes. CHIP 8 Supports 16 levels.
        /// </summary>
        private readonly ushort[] stack = new ushort[16];

        /// <summary>
        /// Byte to point at the top most level of the stack.
        /// </summary>
        private readonly byte stackPointer;

        #endregion

        #region Instance Methods

        /// <summary>
        /// Increments the program counter by two and reads the next word from RAM.
        /// </summary>
        /// <returns>The ushort next instruction to run.</returns>
        public ushort GetNextInstruction()
        {
            var msb = ram[programCounter++];
            var lsb = ram[programCounter++];

            var instruction = (ushort)((msb << 8) | lsb);

            return instruction;
        }

        public void LoadProgram(byte[] data)
        {
            if (data.Length > 4096 - 512)
            {
                throw new ArgumentException("Data must be less than or equal to 4096 -512 bytes in length.");
            }

            //Copy the data provided to the ram. Starting at the 512 mark.
            Array.Copy(data,
                       0,
                       ram,
                       512,
                       data.Length);
        }

        public void Start()
        {
            while (programCounter < 4096)
            {
                var nextInstruction = new Instruction(GetNextInstruction());

                //TODO parse the op code and provide to the proper method
            }

            Debug.Write("Execution complete.");
        }

        #endregion
    }
}
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
        private ushort iRegister;

        private readonly byte[] ram = new byte[4096];

        /// <summary>
        /// Stack is an array of 16 16 bit values. Stores the address to return to when a sub routine finishes. CHIP 8 Supports 16 levels.
        /// We have a length 17 here because it starts at 0. But we increment before setting the stack ever. I.E. the first Call we save the PC in stack[1].
        /// </summary>
        private readonly ushort[] stack = new ushort[17];

        /// <summary>
        /// Byte to point at the top most level of the stack.
        /// </summary>
        private byte stackPointer;

        private readonly Random random = new Random();

        #endregion

        #region Instance Methods

        /// <summary>
        /// Increments the program counter by two and reads the next word from RAM.
        /// </summary>
        /// <returns>
        /// The ushort next instruction to run.
        /// </returns>
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

                var firstHex = nextInstruction.FirstHex;

                //TODO parse the op code and provide to the proper method
                switch (firstHex)
                {
                    case 0x0:
                        switch (nextInstruction.instruction)
                        {
                            case 0x00E0:
                                //TODO clear display
                                break;
                            case 0x00EE:
                                // Return from sub routine
                                programCounter = stack[stackPointer];
                                stackPointer -= 1;
                                break;
                        }

                        break;
                    case 0x1:
                        // Jump to addr
                        programCounter = iRegister;
                        break;
                    case 0x2:
                        // Call addr
                        stackPointer += 0x1;
                        stack[stackPointer] = programCounter;
                        programCounter = nextInstruction.addr;
                        break;
                    case 0x3:
                        // Skip next instruction if vx = kk
                        if (generalRegisters[nextInstruction.x] == nextInstruction.kk)
                        {
                            programCounter += 0x2;
                        }
                        break;
                    case 0x4:
                        // Skip next instruction if vx != kk
                        if (generalRegisters[nextInstruction.x] != nextInstruction.kk)
                        {
                            programCounter += 0x2;
                        }
                        break;
                    case 0x5:
                        //Skip next instruction if vx = vy
                        if (generalRegisters[nextInstruction.x] == generalRegisters[nextInstruction.y])
                        {
                            programCounter += 0x2;
                        }
                        break;
                    case 0x6:
                        // Load kk into register vx
                        generalRegisters[nextInstruction.x] = nextInstruction.kk;
                        break;
                    case 0x7:
                        // Add kk to vx then store in vx
                        generalRegisters[nextInstruction.x] += nextInstruction.kk;
                        break;
                    case 0x8:
                        //TODO multiple ops based on final digit (n)
                        break;
                    case 0x9:
                        // Skip next instruction if vx != vy
                        if (generalRegisters[nextInstruction.x] != generalRegisters[nextInstruction.y])
                        {
                            programCounter += 0x2;
                        }
                        break;
                    case 0xA:
                        // Set I to nnn (addr)
                        iRegister = nextInstruction.addr;
                        break;
                    case 0xB:
                        //Set PC to nnn + v0
                        programCounter = (ushort)(generalRegisters[0] + nextInstruction.addr);
                        break;
                    case 0xC:
                        // RND, gen random between 0 - 255. Rand & kk -> vx
                        var randomVal = new byte[1];
                        random.NextBytes(randomVal);

                        generalRegisters[nextInstruction.x] = (byte)(randomVal[0] & nextInstruction.kk);
                        break;
                    case 0xD:
                        //TODO draw
                        /*
                         * Dxyn - DRW Vx, Vy, nibble
    Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.

    The interpreter reads n bytes from memory, starting at the address stored in I. These bytes are then displayed as sprites on screen at coordinates (Vx, Vy). Sprites are XORed onto the existing screen. If this causes any pixels to be erased, VF is set to 1, otherwise it is set to 0. If the sprite is positioned so part of it is outside the coordinates of the display, it wraps around to the opposite side of the screen. See instruction 8xy3 for more information on XOR, and section 2.4, Display, for more information on the Chip-8 screen and sprites.
                         */
                        break;
                    case 0xE:
                        //TODO two keyboard commands based on last byte of instruction (kk)
                        break;
                    case 0xF:
                        //TODO multiple instructions
                        break;
                    default:
                        Debug.Write($"Unknown code {nextInstruction}.");
                        break;
                }
            }

            Debug.Write("Execution complete.");
        }

        #endregion
    }
}
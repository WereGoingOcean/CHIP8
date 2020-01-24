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
                        switch (nextInstruction.nibble)
                        {
                            case 0x0:
                                // Store vy in vx
                                generalRegisters[nextInstruction.x] = generalRegisters[nextInstruction.y];
                                break;
                            case 0x1:
                                // Bitwise or on vx and vy, stored in vx
                                generalRegisters[nextInstruction.x] = (byte)(generalRegisters[nextInstruction.x] | generalRegisters[nextInstruction.y]);
                                break;
                            case 0x2:
                                // Bitwise and on vx and vy, stored in vx
                                generalRegisters[nextInstruction.x] = (byte)(generalRegisters[nextInstruction.x] & generalRegisters[nextInstruction.y]);
                                break;
                            case 0x3:
                                // Bitwise xor on vx and vy, stored in vx
                                generalRegisters[nextInstruction.x] = (byte)(generalRegisters[nextInstruction.x] ^ generalRegisters[nextInstruction.y]);
                                break;
                            case 0x4:
                                var x = nextInstruction.x;

                                // Add vx and vy. Set vf = 1 if carry (> 8 bit result) otherwise set to 0. Keep lower 8 bits in vx.
                                var result = (ushort)(generalRegisters[x] + generalRegisters[nextInstruction.y]);

                                if ((byte)(result >> 8) > 0x0) //Shift upper bytes then cast to remove all but lower 8 bits
                                {
                                    generalRegisters[0xF] = 0x1;
                                }
                                else
                                {
                                    generalRegisters[0xF] = 0x0;
                                }

                                generalRegisters[x] = (byte)result; // Casting to byte should only take lower bits
                                break;
                            case 0x5:
                                x = nextInstruction.x;

                                var vy = generalRegisters[nextInstruction.y];
                                var vx = generalRegisters[x];

                                // Sub vy from vx. Store result in vx. If vx > vy set vf = 1, otherwise set vf = 0
                                generalRegisters[0xF] = vx > vy
                                                            ? (byte)0x1
                                                            : (byte)0x0;

                                generalRegisters[x] = (byte)(vx - vy);
                                break;
                            case 0x6:
                                x = nextInstruction.x;
                                vx = generalRegisters[x];

                                // If the least - significant bit of Vx is 1, then VF is set to 1, otherwise 0.Then Vx is divided by 2.
                                generalRegisters[0xF] = (byte)(vx & 0x1);

                                generalRegisters[x] = (byte)(vx / 2);
                                break;
                            case 0x7:
                                x = nextInstruction.x;
                                vx = generalRegisters[x];
                                vy = generalRegisters[nextInstruction.y];

                                // If Vy > Vx, then VF is set to 1, otherwise 0. Then Vx is subtracted from Vy, and the results stored in Vx.
                                generalRegisters[0xF] = vy > vx
                                                            ? (byte)0x1
                                                            : (byte)0x0;

                                generalRegisters[x] = (byte)(vy - vx);
                                break;
                            case 0xE:
                                x = nextInstruction.x;
                                vx = generalRegisters[x];

                                // If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2.
                                generalRegisters[0xF] = (byte)((vx & 0x80) >> 7); // Mask all but highest bit, then shift left to get 0 or 1

                                generalRegisters[x] = (byte)(vx / 2);
                                break;
                        }
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
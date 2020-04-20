using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CHIP8Core
{
    public class CHIP8
    {
        #region Fields

        /// <summary>
        /// 16 bit register for program counter. Not accessible by programs. Typically starts at 512 (0x200). Some programs (ETI 600) start
        /// at 1536 (0x600). Not sure if the chip knows how to tell.
        /// </summary>
        private ushort programCounter = 0x200;

        private readonly CancellationTokenSource clockToken = new CancellationTokenSource();

        private readonly ManualResetEventSlim keypressEvent = new ManualResetEventSlim(false);

        private readonly HashSet<byte> pressedKeys = new HashSet<byte>();

        private readonly byte[] ram = new byte[4096];

        private readonly Random random = new Random();

        private readonly IRegisterModule registerModule;

        private readonly TimeSpan sixtySeconds = TimeSpan.FromSeconds(1.0 / 60.0);

        /// <summary>
        /// Stack is an array of 16 16 bit values. Stores the address to return to when a sub routine finishes. CHIP 8 Supports 16 levels.
        /// We have a length 17 here because it starts at 0. But we increment before setting the stack ever. I.E. the first Call we save
        /// the PC in stack[1].
        /// </summary>
        private readonly ushort[] stack = new ushort[17];

        private readonly Action<bool[,]> updateDisplay;

        /// <summary>Decrements at 60hz</summary>
        private byte delayTimer;

        private bool[,] displayPixels = new bool[64, 32];

        private byte lastPressedKey;

        /// <summary>
        /// Decrements at 60hz. Buzzer sounds when > 0.
        /// </summary>
        private byte soundTimer;

        /// <summary>
        /// Byte to point at the top most level of the stack.
        /// </summary>
        private byte stackPointer;

        #endregion

        #region Constructors

        public CHIP8(Action<bool[,]> writeDisplay,
                     IRegisterModule registerModule)
        {
            updateDisplay = writeDisplay;
            this.registerModule = registerModule;
        }

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

        public void KeyPressed(byte key)
        {
            pressedKeys.Add(key);
            lastPressedKey = key;
            keypressEvent.Set();
        }

        public void KeyReleased(byte key)
        {
            pressedKeys.Remove(key);
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
            LoadSprites();

            Task.Run(() => Clock(clockToken.Token));

            while (programCounter < 4096
                   && !clockToken.IsCancellationRequested)
            {
                var nextInstruction = new Instruction(GetNextInstruction());

                var firstHex = nextInstruction.FirstHex;

                // Parse the op code and provide to the proper method
                switch (firstHex)
                {
                    case 0x0:
                        switch (nextInstruction.instruction)
                        {
                            case 0x00E0:
                                // Clear the display
                                displayPixels = new bool[64, 32];
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
                        programCounter = nextInstruction.addr;
                        break;
                    case 0x2:
                        // Call addr
                        stackPointer += 0x1;
                        stack[stackPointer] = programCounter;
                        programCounter = nextInstruction.addr;
                        break;
                    case 0x3:
                        // Skip next instruction if vx = kk
                        if (registerModule.GetGeneralValue(nextInstruction.x) == nextInstruction.kk)
                        {
                            programCounter += 0x2;
                        }

                        break;
                    case 0x4:
                        // Skip next instruction if vx != kk
                        if (registerModule.GetGeneralValue(nextInstruction.x) != nextInstruction.kk)
                        {
                            programCounter += 0x2;
                        }

                        break;
                    case 0x5:
                        //Skip next instruction if vx = vy
                        if (registerModule.GetGeneralValue(nextInstruction.x) == registerModule.GetGeneralValue(nextInstruction.y))
                        {
                            programCounter += 0x2;
                        }

                        break;
                    case 0x6:
                        // Load kk into register vx
                        registerModule.SetGeneralValue(nextInstruction.x,
                                                       nextInstruction.kk);
                        break;
                    case 0x7:
                        // Add kk to vx then store in vx
                        registerModule.SetGeneralValue(nextInstruction.x,
                                                       (byte)(registerModule.GetGeneralValue(nextInstruction.x) + nextInstruction.kk));
                        break;
                    case 0x8:
                        // Arithmetic op codes
                        switch (nextInstruction.nibble)
                        {
                            case 0x0:
                                // Store vy in vx
                                registerModule.SetGeneralValue(nextInstruction.x,
                                                               registerModule.GetGeneralValue(nextInstruction.y));
                                break;
                            case 0x1:
                                // Bitwise or on vx and vy, stored in vx
                                registerModule.SetGeneralValue(nextInstruction.x,
                                                               (byte)(registerModule.GetGeneralValue(nextInstruction.x) | registerModule.GetGeneralValue(nextInstruction.y)));
                                break;
                            case 0x2:
                                // Bitwise and on vx and vy, stored in vx
                                registerModule.SetGeneralValue(nextInstruction.x,
                                                               (byte)(registerModule.GetGeneralValue(nextInstruction.x) & registerModule.GetGeneralValue(nextInstruction.y)));
                                break;
                            case 0x3:
                                // Bitwise xor on vx and vy, stored in vx
                                registerModule.SetGeneralValue(nextInstruction.x,
                                                               (byte)(registerModule.GetGeneralValue(nextInstruction.x) ^ registerModule.GetGeneralValue(nextInstruction.y)));
                                break;
                            case 0x4:
                                var x = nextInstruction.x;

                                // Add vx and vy. Set vf = 1 if carry (> 8 bit result) otherwise set to 0. Keep lower 8 bits in vx.
                                var result = (ushort)(registerModule.GetGeneralValue(x) + registerModule.GetGeneralValue(nextInstruction.y));

                                if ((byte)(result >> 8) > 0x0) //Shift upper bytes then cast to remove all but lower 8 bits
                                {
                                    registerModule.SetGeneralValue(0xF,
                                                                   0x1);
                                }
                                else
                                {
                                    registerModule.SetGeneralValue(0xF,
                                                                   0x0);
                                }

                                registerModule.SetGeneralValue(x,
                                                               (byte)result); // Casting to byte should only take lower bits
                                break;
                            case 0x5:
                                x = nextInstruction.x;

                                var vy = registerModule.GetGeneralValue(nextInstruction.y);
                                var vx = registerModule.GetGeneralValue(x);

                                // Sub vy from vx. Store result in vx. If vx > vy set vf = 1, otherwise set vf = 0
                                registerModule.SetGeneralValue(0xF,
                                                               vx > vy
                                                                   ? (byte)0x1
                                                                   : (byte)0x0);

                                registerModule.SetGeneralValue(x,
                                                               (byte)(vx - vy));
                                break;
                            case 0x6:
                                x = nextInstruction.x;
                                vx = registerModule.GetGeneralValue(x);

                                // If the least - significant bit of Vx is 1, then VF is set to 1, otherwise 0.Then Vx is divided by 2.
                                registerModule.SetGeneralValue(0xF,
                                                               (byte)(vx & 0x1));

                                registerModule.SetGeneralValue(x,
                                                               (byte)(vx / 2));
                                break;
                            case 0x7:
                                x = nextInstruction.x;
                                vx = registerModule.GetGeneralValue(x);
                                vy = registerModule.GetGeneralValue(nextInstruction.y);

                                // If Vy > Vx, then VF is set to 1, otherwise 0. Then Vx is subtracted from Vy, and the results stored in Vx.
                                registerModule.SetGeneralValue(0xF,
                                                               vy > vx
                                                                   ? (byte)0x1
                                                                   : (byte)0x0);

                                registerModule.SetGeneralValue(x,
                                                               (byte)(vy - vx));
                                break;
                            case 0xE:
                                x = nextInstruction.x;
                                vx = registerModule.GetGeneralValue(x);

                                // If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2.
                                registerModule.SetGeneralValue(0xF,
                                                               (byte)((vx & 0x80) >> 7)); // Mask all but highest bit, then shift left to get 0 or 1

                                registerModule.SetGeneralValue(x,
                                                               (byte)(vx / 2));
                                break;
                        }

                        break;
                    case 0x9:
                        // Skip next instruction if vx != vy
                        if (registerModule.GetGeneralValue(nextInstruction.x) != registerModule.GetGeneralValue(nextInstruction.y))
                        {
                            programCounter += 0x2;
                        }

                        break;
                    case 0xA:
                        // Set I to nnn (addr)
                        registerModule.SetI(nextInstruction.addr);
                        break;
                    case 0xB:
                        //Set PC to nnn + v0
                        programCounter = (ushort)(registerModule.GetGeneralValue(0) + nextInstruction.addr);
                        break;
                    case 0xC:
                        // RND, gen random between 0 - 255. Rand & kk -> vx
                        var randomVal = new byte[1];
                        random.NextBytes(randomVal);

                        registerModule.SetGeneralValue(nextInstruction.x,
                                                       (byte)(randomVal[0] & nextInstruction.kk));
                        break;
                    case 0xD:

                        // Draw
                        /*
                         * Dxyn - DRW Vx, Vy, nibble
                                Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.

                                The interpreter reads n bytes from memory, starting at the address stored in I. These bytes are then displayed as sprites on screen at coordinates (Vx, Vy). Sprites are XORed onto the existing screen. If this causes any pixels to be erased, VF is set to 1, otherwise it is set to 0. If the sprite is positioned so part of it is outside the coordinates of the display, it wraps around to the opposite side of the screen. See instruction 8xy3 for more information on XOR, and section 2.4, Display, for more information on the Chip-8 screen and sprites.
                         */
                        bool[] ConvertByteToBoolArray(byte b)
                        {
                            // Prepare the return result
                            var result = new bool[8];

                            // Check each bit in the byte. if 1 set to true, if 0 set to false
                            for (var i = 0; i < 8; i++)
                            {
                                result[i] = (b & (1 << i)) != 0;
                            }

                            // Reverse the array
                            Array.Reverse(result);

                            return result;
                        }

                        var xVal = nextInstruction.x;
                        var y = nextInstruction.y;

                        var setVf = false;

                        for (var i = 0; i < nextInstruction.nibble; i++)
                        {
                            var row = y + i;

                            var sprite = ram[registerModule.GetI() + i];

                            var pixelSetting = ConvertByteToBoolArray(sprite);

                            for (var k = 0; k < 8; k++)
                            {
                                var column = xVal + k;

                                if (column > 63)
                                {
                                    column -= 63;
                                }

                                var newSetting = displayPixels[column,
                                                               row]
                                                 ^ pixelSetting[k];

                                if (!newSetting)
                                {
                                    setVf = true;
                                }

                                displayPixels[column,
                                              row] = newSetting;
                            }
                        }

                        registerModule.SetGeneralValue(0xF,
                                                       setVf
                                                           ? (byte)0x1
                                                           : (byte)0x0);

                        updateDisplay.Invoke(displayPixels);

                        break;
                    case 0xE:
                        // Two keyboard commands based on last byte of instruction (kk)
                        switch (nextInstruction.kk)
                        {
                            case 0x9E:
                                /* Skip next instruction if key with the value of Vx is pressed.
                                Checks the keyboard, and if the key corresponding to the value of Vx is currently in the down position, PC is increased by 2. */
                                if (pressedKeys.Contains(registerModule.GetGeneralValue(nextInstruction.x)))
                                {
                                    programCounter += 0x2;
                                }

                                break;
                            case 0xA1:
                                /* Skip next instruction if key with the value of Vx is not pressed.
                                Checks the keyboard, and if the key corresponding to the value of Vx is currently in the up position, PC is increased by 2. */
                                if (!pressedKeys.Contains(registerModule.GetGeneralValue(nextInstruction.x)))
                                {
                                    programCounter += 0x2;
                                }

                                break;
                        }

                        break;
                    case 0xF:
                        switch (nextInstruction.kk)
                        {
                            case 0x07:
                                // Set vx = delay timer
                                registerModule.SetGeneralValue(nextInstruction.x,
                                                               delayTimer);
                                break;
                            case 0x0A:
                                // Wait for key press, store key value in vx
                                // All execution stops TODO even timer?
                                keypressEvent.Reset();
                                keypressEvent.Wait();
                                registerModule.SetGeneralValue(nextInstruction.x,
                                                               lastPressedKey);
                                break;
                            case 0x15:
                                // Set DT = vx
                                delayTimer = registerModule.GetGeneralValue(nextInstruction.x);
                                break;
                            case 0x18:
                                // Set ST = vx
                                soundTimer = registerModule.GetGeneralValue(nextInstruction.x);
                                break;
                            case 0x1E:
                                // Set I = I + vx
                                registerModule.SetI((ushort)(registerModule.GetGeneralValue(nextInstruction.x) + registerModule.GetI()));
                                break;
                            case 0x29:
                                // Set I = location of sprite for digit Vx.
                                var vx = (int)registerModule.GetGeneralValue(nextInstruction.x);

                                var offset = vx / 5;

                                registerModule.SetI((byte)offset);
                                break;
                            case 0x33:
                                /* Store BCD representation of Vx in memory locations I, I+1, and I+2.
                                   The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2. */
                                vx = registerModule.GetGeneralValue(nextInstruction.x);

                                var hundreds = vx / 100;
                                var tens = vx / 10 % 10;
                                var ones = vx % 10;

                                var iVal = registerModule.GetI();

                                ram[iVal] = (byte)hundreds;
                                ram[iVal + 1] = (byte)tens;
                                ram[iVal + 2] = (byte)ones;
                                break;
                            case 0x55:
                                var x = nextInstruction.x;
                                /* Store registers V0 through Vx in memory starting at location I.
                                The interpreter copies the values of registers V0 through Vx into memory, starting at the address in I. */
                                for (var i = 0; i < x; x++)
                                {
                                    iVal = registerModule.GetI();

                                    ram[iVal] = registerModule.GetGeneralValue(i);
                                    registerModule.SetI((ushort)(iVal + 1));
                                }

                                break;
                            case 0x65:
                                x = nextInstruction.x;
                                /* Read registers V0 through Vx from memory starting at location I.
                                The interpreter reads values from memory starting at location I into registers V0 through Vx. */
                                for (var i = 0; i < x; x++)
                                {
                                    iVal = registerModule.GetI();

                                    registerModule.SetGeneralValue(i,
                                                                   ram[iVal]);
                                    registerModule.SetI((ushort)(iVal + 1));
                                }

                                break;
                        }

                        break;
                    default:
                        Trace.WriteLine($"Unknown code {nextInstruction}. Ending execution.");
                        return;
                }
            }

            Trace.WriteLine("Execution complete.");
        }

        public void Stop()
        {
            clockToken.Cancel();
        }

        private async void Clock(CancellationToken cancellationToken)
        {
            var lastClockTick = DateTime.Now;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (DateTime.Now.Subtract(lastClockTick) >= sixtySeconds)
                {
                    //TODO action for beep & un-beep

                    if (soundTimer > 0)
                    {
                        soundTimer--;
                    }

                    if (delayTimer > 0)
                    {
                        delayTimer--;
                    }
                }

                await Task.Delay(1_000);
            }
        }

        private void LoadSprites()
        {
            // 0
            ram[0] = 0xF0;
            ram[1] = 0x90;
            ram[2] = 0x90;
            ram[3] = 0x90;
            ram[4] = 0xF0;
            // 1
            ram[5] = 0x20;
            ram[6] = 0x60;
            ram[7] = 0x20;
            ram[8] = 0x20;
            ram[9] = 0x70;
            // 2
            ram[10] = 0xF0;
            ram[11] = 0x10;
            ram[12] = 0xF0;
            ram[13] = 0x80;
            ram[14] = 0xF0;
            // 3
            ram[15] = 0xF0;
            ram[16] = 0x10;
            ram[17] = 0xF0;
            ram[18] = 0x10;
            ram[19] = 0xF0;
            // 4
            ram[20] = 0x90;
            ram[21] = 0x90;
            ram[22] = 0xF0;
            ram[23] = 0x10;
            ram[24] = 0x10;
            // 5
            ram[25] = 0xF0;
            ram[26] = 0x80;
            ram[27] = 0xF0;
            ram[28] = 0x10;
            ram[29] = 0xF0;
            // 6
            ram[30] = 0xF0;
            ram[31] = 0x80;
            ram[32] = 0xF0;
            ram[33] = 0x90;
            ram[34] = 0xF0;
            // 7
            ram[35] = 0xF0;
            ram[36] = 0x10;
            ram[37] = 0x20;
            ram[38] = 0x40;
            ram[39] = 0x40;
            // 8
            ram[40] = 0xF0;
            ram[41] = 0x90;
            ram[42] = 0xF0;
            ram[43] = 0x90;
            ram[44] = 0xF0;
            // 9
            ram[45] = 0xF0;
            ram[46] = 0x90;
            ram[47] = 0xF0;
            ram[48] = 0x10;
            ram[49] = 0xF0;
            // A
            ram[50] = 0xF0;
            ram[51] = 0x90;
            ram[52] = 0xF0;
            ram[53] = 0x90;
            ram[54] = 0x90;
            // B
            ram[55] = 0xE0;
            ram[56] = 0x90;
            ram[57] = 0xE0;
            ram[58] = 0x90;
            ram[59] = 0xE0;
            // C
            ram[60] = 0xF0;
            ram[61] = 0x80;
            ram[62] = 0x80;
            ram[63] = 0x80;
            ram[64] = 0xF0;
            // D
            ram[65] = 0xE0;
            ram[66] = 0x90;
            ram[67] = 0x90;
            ram[68] = 0x90;
            ram[69] = 0xE0;
            // E
            ram[70] = 0xF0;
            ram[71] = 0x80;
            ram[72] = 0xF0;
            ram[73] = 0x80;
            ram[74] = 0xF0;
            // F
            ram[75] = 0xF0;
            ram[76] = 0x80;
            ram[77] = 0xF0;
            ram[78] = 0x80;
            ram[79] = 0x80;
        }

        #endregion
    }
}
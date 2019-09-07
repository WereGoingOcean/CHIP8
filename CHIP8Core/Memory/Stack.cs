using CHIP8Core.Models;

namespace CHIP8Core.Memory
{
    public class Stack
    {
        #region Fields

        private readonly TwoBytes[] stackMemory = new TwoBytes[16];

        private int stackPointer = -1;

        #endregion

        #region Instance Methods

        public TwoBytes Pop()
        {
            var valueToReturn = stackMemory[stackPointer];

            stackPointer--;

            return valueToReturn;
        }

        public void Push(TwoBytes valueToPush)
        {
            stackPointer++;

            stackMemory[stackPointer] = valueToPush;
        }

        #endregion
    }
}
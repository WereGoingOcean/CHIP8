namespace CHIP8Core
{
    public interface IStackModule
    {
        #region Instance Methods

        ushort Pop();

        void Push(ushort value);

        #endregion
    }

    public class StackModule : IStackModule
    {
        #region Fields

        /// <summary>
        /// Stack is an array of 16 16 bit values. Stores the address to return to when a sub routine finishes. CHIP 8 Supports 16 levels.
        /// We have a length 17 here because it starts at 0. But we increment before setting the stack ever. I.E. the first Call we save
        /// the PC in stack[1].
        /// </summary>
        private readonly ushort[] stack = new ushort[17];

        /// <summary>
        /// Byte to point at the top most level of the stack.
        /// </summary>
        private byte stackPointer;

        #endregion

        #region Instance Methods

        public ushort Pop()
        {
            var value = stack[stackPointer];
            stackPointer -= 0x1;
            return value;
        }

        public void Push(ushort value)
        {
            stackPointer += 0x1;
            stack[stackPointer] = value;
        }

        #endregion
    }
}
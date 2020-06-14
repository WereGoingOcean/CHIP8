using System;

namespace CHIP8Core
{
    public interface IRandomModule
    {
        #region Instance Methods

        byte GetNextRandom();

        #endregion
    }

    public class RandomModule : IRandomModule
    {
        #region Fields

        private readonly Random random = new Random();

        #endregion

        #region Instance Methods

        public byte GetNextRandom()
        {
            var randomVal = new byte[1];
            random.NextBytes(randomVal);
            return randomVal[0];
        }

        #endregion
    }
}
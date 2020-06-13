using System;
using System.Collections.Generic;
using System.Linq;

namespace CHIP8Core
{
    /// <summary>
    /// Basically just a wrapper around the ram.
    /// </summary>
    public class MemoryModule
    {
        #region Constants

        private const int RamLength = 4096;

        #endregion

        #region Fields

        private readonly byte[] ram;

        #endregion

        #region Constructors

        public MemoryModule(IEnumerable<byte> ram)
        {
            if (ram.Count() != RamLength)
            {
                throw new ArgumentException($"Ram is expected to be {RamLength} bytes long.");
            }

            // Create a new copy instead of a reference
            this.ram = new List<byte>(ram).ToArray();
        }

        #endregion

        #region Instance Indexers

        public byte this[int index]
        {
            get
            {
                return ram[index];
            }
            set
            {
                ram[index] = value;
            }
        }

        #endregion

        #region Class Methods

        public static implicit operator byte[](MemoryModule memoryModule)
        {
            // Implicit operator so we can do Array.Copy to load the program.
            return memoryModule.ram;
        }

        #endregion
    }
}
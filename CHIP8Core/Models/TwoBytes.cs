using System;

namespace CHIP8Core.Models
{
    public class TwoBytes
    {
        #region Constructors

        public TwoBytes(byte mostSignificant,
                        byte leastSignificant)
        {
            MostSignificant = mostSignificant;
            LeastSignificant = leastSignificant;
        }

        #endregion

        #region Instance Properties

        public byte LeastSignificant { get; }

        public byte MostSignificant { get; }

        #endregion

        #region Instance Methods

        public void Deconstruct(out byte mostSignificant,
                                out byte leastSignificant)
        {
            mostSignificant = MostSignificant;
            leastSignificant = LeastSignificant;
        }

        #endregion

        #region Class Methods

        public static implicit operator int(TwoBytes twoBytes)
        {
            return BitConverter.ToInt16(new[]
                                        {
                                            twoBytes.MostSignificant,
                                            twoBytes.LeastSignificant
                                        },
                                        0);
        }

        public static implicit operator TwoBytes(int value)
        {
            var byteArray = BitConverter.GetBytes(value);

            //TODO not 100% sure these are correct
            if (littleEndian)
            {
                return new TwoBytes(byteArray[1],
                                    byteArray[0]);
            }
            else
            {
                return new TwoBytes(byteArray[2],
                                    byteArray[3]);
            }
        }

        private static readonly bool littleEndian = BitConverter.IsLittleEndian;

        #endregion
    }
}
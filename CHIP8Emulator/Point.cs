using System;

namespace CHIP8Emulator
{
    public class Point
    {
        #region Constructors

        public Point(int x,
                     int y)
        {
            X = x;
            Y = y;
        }

        #endregion

        #region Instance Properties

        public int X { get; }

        public int Y { get; }

        #endregion

        #region Instance Methods

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null,
                                obj))
            {
                return false;
            }

            if (ReferenceEquals(this,
                                obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(Point))
            {
                return false;
            }

            return Equals((Point)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X,
                                    Y);
        }

        protected bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        #endregion
    }
}
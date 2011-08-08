using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cs2dClientBot
{
    public class Vector2D
    {
        private float x, y;

        public Vector2D(float x, float y)
        {
            this.x = x; this.y = y;
        }

        public Vector2D(int x, int y)
        {
            this.x = (float)x; this.y = (float)y;
        }

        public float length()
        {
            return (float)Math.Sqrt((double)(x * x + y * y));
        }

        public float X
        {
            get { return x; }
        }
        public float Y
        {
            get { return y; }
        }

        public Vector2D Uniform()
        {
            float length = this.length();
            
            bool xLargest = Math.Abs(x) > Math.Abs(y);

            if (xLargest && x > 0.0f)
                return new Vector2D(1.0f, 0);
            else if (xLargest && x < 0.0f)
                return new Vector2D(-1.0f, 0);
            else if (!xLargest && y > 0.0f)
                return new Vector2D(0, 1.0f);
            else if (!xLargest && y < 0.0f)
                return new Vector2D(0, -1.0f);
            else
                return new Vector2D(0, 0);
        }
        

    }
}

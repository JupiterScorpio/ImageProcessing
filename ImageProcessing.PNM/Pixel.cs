﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO
{
    public struct Pixel
    {
        public static Pixel Black = new Pixel(0, 0, 0);
        public static Pixel White = new Pixel(255, 255, 255);

        private readonly byte red;
        private readonly byte green;
        private readonly byte blue;

        public byte Red { get { return red; } }
        public byte Green { get { return green; } }
        public byte Blue { get { return blue; } }
        
        public Pixel(byte red, byte green, byte blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

    }
}

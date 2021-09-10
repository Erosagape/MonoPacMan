using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
namespace MyPacMan
{
    public static class Grid
    {
        private static int width = 28;
        private static int height = 31;
        static Tile[,] tileGrid = new Tile[width, height];
        public static Tile[,] TileGrid
        {
            get { return tileGrid; }
        }
        public static int Width
        {
            get { return width; }
        }
        public static int Height
        {
            get { return height; }
        }
        public static int NumCrumps { get; set; }
        static Grid()
        {

        }
        static void InitializeFromFile()
        {
            var tr = new StreamReader("Content/Grid.txt");
            string line = tr.ReadLine();
            int lineIndex = 0, charIndex = 0;
            while (line != null)
            {
                foreach(char c in line)
                {
                    switch (c)
                    {
                        case '0':
                            TileGrid[charIndex, lineIndex] = new Tile(TileTypes.Closed, false, false, new Point(charIndex,lineIndex));
                            break;
                        case '1':
                            TileGrid[charIndex, lineIndex] = new Tile(TileTypes.Open, true, false, new Point(charIndex, lineIndex));
                            break;
                        case '2':
                            TileGrid[charIndex, lineIndex] = new Tile(TileTypes.Home, false, false, new Point(charIndex, lineIndex));
                            break;
                        case '3':
                            TileGrid[charIndex, lineIndex] = new Tile(TileTypes.Open, true, true, new Point(charIndex, lineIndex));
                            break;
                    }
                    if (c != ' ')
                        charIndex++;
                }
                charIndex = 0;
                lineIndex++;
                line = tr.ReadLine();
            }
            tr.Close();
            SetCrumps();
        }
        static void SetCrumps()
        {
            for (int i = 0; i < 28; i++)
            {
                if (i != 6 && i != 21)
                {
                    TileGrid[i, 14].HasCrump = false;
                }
            }
            for (int i = 11; i < 20; i++)
            {
                TileGrid[9, i].HasCrump = false;
                TileGrid[18, i].HasCrump = false;
            }
            for (int i = 10; i < 18; i++)
            {
                TileGrid[i, 11].HasCrump = false;
                TileGrid[i, 17].HasCrump = false;
            }
            TileGrid[12, 9].HasCrump = false;
            TileGrid[15, 9].HasCrump = false;
            TileGrid[12, 10].HasCrump = false;
            TileGrid[15, 10].HasCrump = false;
            TileGrid[13, 23].HasCrump = false;
            TileGrid[14, 23].HasCrump = false;
        }
        public static void Reset()
        {
            NumCrumps = 0;
            InitializeFromFile();
        }
    }
}

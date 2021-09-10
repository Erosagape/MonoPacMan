using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacMan
{
    public struct Position
    {
        public Point Tile;
        public Point DeltaPixel;
        public Position(Point tile, Point deltaPixel)
        {
            Tile = tile;
            DeltaPixel = deltaPixel;
        }
    }
    public enum Direction { Up, Down, Left, Right };
    public enum State { Start, Normal, Dying };
    public enum GhostState { Home, Scatter, Attack, Blue, Dead }
    public enum Ghosts { Blinky, Pinky, Inky, Clyde }
    public enum TileTypes { Open,Closed,Home };
    public struct ScoreEvent
    {
        public Position Position;
        public DateTime When;
        public int Score;
        public ScoreEvent(Position position, DateTime when, int score)
        {
            Position = position;
            When = when;
            Score = score;
        }
    }
    /// <summary>
    /// A square of the maze
    /// </summary>
    public struct Tile
    {

        TileTypes type;
        /// <summary>
        /// The type of the tile
        /// </summary>
        public TileTypes Type
        {
            get { return type; }
            set { type = value; }
        }

        bool hasCrump;
        /// <summary>
        /// Whether the tile has a crump
        /// </summary>
        public bool HasCrump
        {
            get { return hasCrump; }
            set
            {
                if (value != hasCrump)
                {
                    Grid.NumCrumps += value ? 1 : -1;
                }
                hasCrump = value;
            }
        }

        bool hasPowerPill;
        /// <summary>
        /// Whether the tile has a power pill
        /// </summary>
        public bool HasPowerPill
        {
            get { return hasPowerPill; }
            set { hasPowerPill = value; }
        }

        public bool IsOpen
        {
            get { return type == TileTypes.Open; }
        }

        Point position;
        public Point ToPoint { get { return position; } }

        /// <summary>
        /// Sets the different attributes
        /// </summary>
        /// <param name="type">The type of tile</param>
        /// <param name="hasCrump">Whether the tile has a crump</param>
        /// <param name="hasPowerPill">Whether the tile has a power pill</param>
        public Tile(TileTypes type, bool hasCrump, bool hasPowerPill, Point position)
        {
            this.type = type;
            this.hasCrump = hasCrump;
            if (hasCrump)
            {
                Grid.NumCrumps++;
            }
            this.hasPowerPill = hasPowerPill;
            this.position = position;
        }
    }
}

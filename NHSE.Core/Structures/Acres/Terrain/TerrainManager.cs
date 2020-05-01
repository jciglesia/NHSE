﻿using System.Diagnostics;

namespace NHSE.Core
{
    public class TerrainManager : MapGrid
    {
        public readonly TerrainTile[] Tiles;

        public TerrainManager(TerrainTile[] tiles) : base(16, 16)
        {
            Tiles = tiles;
            Debug.Assert(MapTileCount == tiles.Length);
        }

        public TerrainTile GetTile(int x, int y) => this[GetTileIndex(x, y)];
        public TerrainTile GetTile(int acreX, int acreY, int gridX, int gridY) => this[GetTileIndex(acreX, acreY, gridX, gridY)];
        public TerrainTile GetAcreTile(int acreIndex, int tileIndex) => this[GetAcreTileIndex(acreIndex, tileIndex)];

        public TerrainTile this[int index]
        {
            get => Tiles[index];
            set => Tiles[index] = value;
        }

        public byte[] DumpAllAcres()
        {
            var result = new byte[Tiles.Length * TerrainTile.SIZE];
            for (int i = 0; i < Tiles.Length; i++)
                Tiles[i].ToBytesClass().CopyTo(result, i * TerrainTile.SIZE);
            return result;
        }

        public byte[] DumpAcre(int acre)
        {
            int count = AcreTileCount;
            var result = new byte[TerrainTile.SIZE * count];
            for (int i = 0; i < count; i++)
            {
                var tile = GetAcreTile(acre, i);
                var bytes = tile.ToBytesClass();
                bytes.CopyTo(result, i * TerrainTile.SIZE);
            }
            return result;
        }

        public void ImportAllAcres(byte[] data)
        {
            var tiles = TerrainTile.GetArray(data);
            for (int i = 0; i < tiles.Length; i++)
                Tiles[i].CopyFrom(tiles[i]);
        }

        public void ImportAcre(int acre, byte[] data)
        {
            int count = AcreTileCount;
            var tiles = TerrainTile.GetArray(data);
            for (int i = 0; i < count; i++)
            {
                var tile = GetAcreTile(acre, i);
                tile.CopyFrom(tiles[i]);
            }
        }

        public void SetAll(TerrainTile tile, in bool interiorOnly)
        {
            if (interiorOnly)
            {
                // skip outermost ring of tiles
                int xmin = GridWidth;
                int ymin = GridHeight;
                int xmax = MapWidth - GridWidth;
                int ymax = MapHeight - GridHeight;
                for (int x = xmin; x < xmax; x++)
                {
                    for (int y = ymin; y < ymax; y++)
                        GetTile(x, y).CopyFrom(tile);
                }
            }
            else
            {
                foreach (var t in Tiles)
                    t.CopyFrom(tile);
            }
        }

        public void GetBuildingCoordinate(ushort bx, ushort by, int scale, out int x, out int y)
        {
            // Although there is terrain in the Top Row and Left Column, no buildings can be placed there.
            // Adjust the building coordinates down-right by an acre.
            int buildingShift = GridWidth;
            x = (int)(((bx / 2f) - buildingShift) * scale);
            y = (int)(((by / 2f) - buildingShift) * scale);
        }

        public bool GetBuildingRelativeCoordinate(int topX, int topY, int acreScale, ushort bx, ushort by, out int relX, out int relY)
        {
            GetBuildingCoordinate(bx, by, acreScale, out var x, out var y);
            relX = x - (topX * acreScale);
            relY = y - (topY * acreScale);

            return IsWithinGrid(acreScale, relX, relY);
        }

        private bool IsWithinGrid(int acreScale, int relX, int relY)
        {
            if ((uint)relX >= GridWidth * acreScale)
                return false;

            if ((uint)relY >= GridHeight * acreScale)
                return false;

            return true;
        }
    }
}

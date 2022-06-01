using UnityEngine;

namespace FunkySheep.Earth.Map
{
    public class Tile
    {
        public Vector3Int tilemapPosition;
        public Vector2Int mapPosition;
        public UnityEngine.Tilemaps.Tile data;
        public Tile(Vector3Int tilemapPosition, Vector2Int mapPosition, UnityEngine.Tilemaps.Tile data)
        {
            this.tilemapPosition = tilemapPosition;
            this.mapPosition = mapPosition;
            this.data = data;
        }
    }
}

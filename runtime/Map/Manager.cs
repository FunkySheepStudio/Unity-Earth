using UnityEngine;
using UnityEngine.Tilemaps;

namespace FunkySheep.Earth.Map
{
    [AddComponentMenu("FunkySheep/Map/Map Manager")]
    [RequireComponent(typeof(Tilemap))]
    [RequireComponent(typeof(TilemapRenderer))]
    [RequireComponent(typeof(Grid))]
    public class Manager : MonoBehaviour
    {
        public FunkySheep.Types.Int32 zoomLevel;
        public FunkySheep.Types.Float tileSize;
        public FunkySheep.Types.Vector2 initialOffset;
        public FunkySheep.Types.Vector2 initialMapPosition;

        public FunkySheep.Types.String url;
        public FunkySheep.Types.Vector2Int textureResolution;
        public AddedTileEvent addedTileEvent;
        Tilemap tilemap;

        private void Awake()
        {
            transform.Rotate(new Vector3(90, 0, 0));
            tilemap = GetComponent<Tilemap>();
            GetComponent<Grid>().cellSize = new Vector3(
                textureResolution.value.x,
                textureResolution.value.y,
                1
            );
        }

        /// <summary>
        /// Set the default values
        /// </summary>
        void Reset()
        {
            tilemap.transform.localScale = new Vector3(
                tileSize.value / textureResolution.value.x,
                tileSize.value / textureResolution.value.y,
            1f);

            tilemap.tileAnchor = new Vector3(
                initialOffset.value.x,
                initialOffset.value.y,
                0
            );
        }

        /// <summary>
        /// Add osm tile at given position
        /// </summary>
        /// <param name="mapPosition"></param>
        public void AddTile(Vector2Int mapPosition)
        {
            // Set the default values
            if (!tilemap.HasTile(Vector3Int.zero))
            {
                Reset();
            }

            // Reverse mercator and grid positions
            Vector2Int gridPosition = (mapPosition - new Vector2Int(Mathf.FloorToInt(initialMapPosition.value.x), Mathf.FloorToInt(initialMapPosition.value.y))) * new Vector2Int(1, -1);
            Vector3Int tileMapPosition = new Vector3Int(gridPosition.x, gridPosition.y, 0);

            if (!tilemap.HasTile(tileMapPosition))
            {
                string url = InterpolatedUrl(mapPosition);
                DownloadTile(tileMapPosition, mapPosition, url);
            }
        }

        public void DownloadTile(Vector3Int tileMapPosition, Vector2Int mapPosition, string url)
        {
            StartCoroutine(FunkySheep.Network.Downloader.Download(url, (fileID, file) =>
            {
                Tile tile = new Tile(
                    tileMapPosition,
                    mapPosition,
                    SetTile(file)
                );

                tilemap.SetTile(tile.tilemapPosition, tile.data);

                if (addedTileEvent != null)
                {
                    addedTileEvent.Raise(tile);
                }
            }));
        }

        /// <summary>
        /// Set the tile object
        /// </summary>
        /// <param name="texture">The texture to set on the tile sprite</param>
        public UnityEngine.Tilemaps.Tile SetTile(byte[] textureFile)
        {
            Texture2D texture = new Texture2D(256, 256);
            texture.LoadImage(textureFile);
            UnityEngine.Tilemaps.Tile tileData;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            tileData = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            tileData.sprite = Sprite.Create((Texture2D)texture, new Rect(0.0f, 0.0f, texture.width, texture.height), Vector2.zero, 1);
            return tileData;
        }

        /// <summary>
        /// Interpolate the url inserting the coordinates and zoom values
        /// </summary>
        /// <param name="zoom">The zoom value</param>
        /// <param name="position">The coordinates</param>
        /// <returns>The interpolated Url</returns>
        public string InterpolatedUrl(Vector2Int mapPosition)
        {
            string[] parameters = new string[3];
            string[] parametersNames = new string[3];

            parameters[0] = zoomLevel.value.ToString();
            parametersNames[0] = "zoom";

            parameters[1] = mapPosition.x.ToString();
            parametersNames[1] = "position.x";

            parameters[2] = mapPosition.y.ToString();
            parametersNames[2] = "position.y";

            return url.Interpolate(parameters, parametersNames);
        }
    }
}

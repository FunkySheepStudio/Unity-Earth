using System.Threading;
using UnityEngine;

namespace FunkySheep.Earth.Terrain
{
    [RequireComponent(typeof(UnityEngine.Terrain))]
    [RequireComponent(typeof(UnityEngine.TerrainCollider))]
    public class Tile : MonoBehaviour
    {
        public Vector2Int position;
        public FunkySheep.Earth.Terrain.AddedTileEvent addedTileEvent;
        public UnityEngine.Terrain terrain;
        Color32[] pixels;
        public float[,] heights;
        Texture2D texture;

        public bool heightsCalculated = false;
        public bool heightUpdated = false;
        int terrainResolution;


        private void Awake()
        {
            terrain = GetComponent<UnityEngine.Terrain>();
            terrain.terrainData = new TerrainData();
            GetComponent<UnityEngine.TerrainCollider>().terrainData = terrain.terrainData;
        }

        private void Update()
        {
            if (heightsCalculated && !heightUpdated)
            {
                terrain.terrainData.SetHeightsDelayLOD(0, 0, heights);
                terrain.terrainData.SyncHeightmap();
                heightUpdated = true;
                gameObject.AddComponent<Connector>();
                addedTileEvent.Raise(this);
            }
        }

        /// <summary>
        /// Load Terrarium texture
        /// https://github.com/tilezen/joerd/blob/master/docs/formats.md#terrarium
        /// </summary>
        /// <param name="texture"></param>
        public void SetHeights(Map.Tile tile)
        {
            terrainResolution = terrain.terrainData.heightmapResolution;
            texture = tile.data.sprite.texture;
            pixels = texture.GetPixels32();
            heights = new float[terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution];

            Thread thread = new Thread(() => this.ProcessHeights());

            thread.Start();
        }

        public void SetHeights(float[,] heights)
        {
            terrain.terrainData.SetHeightsDelayLOD(0, 0, heights);
            terrain.terrainData.SyncHeightmap();
        }

        public void ProcessHeights()
        {
            for (float x = 0; x < terrainResolution; x++)
            {
                for (float y = 0; y < terrainResolution; y++)
                {
                    float height = GetHeightFromColor(
                      Mathf.FloorToInt(x / terrainResolution * Mathf.Sqrt(pixels.Length)),
                      Mathf.FloorToInt(y / terrainResolution * Mathf.Sqrt(pixels.Length))
                    );
                    // Convert the resulting color value to an elevation in meters.
                    heights[
                        (int)x,
                        (int)y
                    ] = height;
                }
            }
            heightsCalculated = true;
        }

        public float GetHeightFromColor(int x, int y)
        {
            Color32 color = pixels[
                y +
                x * (int)Mathf.Sqrt(pixels.Length)];

            float height = (Mathf.Floor(color.r * 256.0f) + Mathf.Floor(color.g) + color.b / 256) - 32768.0f;
            height /= 8900;

            return height;
        }
    }
}

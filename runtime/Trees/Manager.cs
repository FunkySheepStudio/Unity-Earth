using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace FunkySheep.Earth.Trees
{
    [AddComponentMenu("FunkySheep/Earth/Earth Trees Manager")]
    public class Manager : MonoBehaviour
    {
        public FunkySheep.Earth.Manager earthManager;
        public FunkySheep.Earth.Map.Manager mapManager;
        public GameObject tree;
        public ConcurrentQueue<Vector3> trees = new ConcurrentQueue<Vector3>();
        public List<Color> authorizedColors = new List<Color>();
        List<Map.Tile> OSMTiles = new List<Map.Tile>();
        List<Map.Tile> satelitesTiles = new List<Map.Tile>();

        private void Awake()
        {
            authorizedColors.Add(new Color(224, 223, 223));
            authorizedColors.Add(new Color(205, 235, 176));
            authorizedColors.Add(new Color(173, 209, 158));
            authorizedColors.Add(new Color(200, 250, 204));
            authorizedColors.Add(new Color(223, 252, 226));
            authorizedColors.Add(new Color(221, 221, 232));
            authorizedColors.Add(new Color(242, 239, 233));
            authorizedColors.Add(new Color(255, 241, 186));
        }

        private void Update()
        {
            Create(50);
        }

        public void AddedOSMTile(Map.Tile OSMtile)
        {
            Map.Tile satelitesTile = satelitesTiles.Find(tile => tile.tilemapPosition == OSMtile.tilemapPosition);
            if (satelitesTile != null)
            {
                ProcessTrees(OSMtile, satelitesTile);
                satelitesTiles.Remove(satelitesTile);
            } else
            {
                OSMTiles.Add(OSMtile);
            }
        }

        public void AddedSatelliteTile(Map.Tile satelliteTile)
        {
            Map.Tile OSMTile = OSMTiles.Find(tile => tile.tilemapPosition == satelliteTile.tilemapPosition);
            if (OSMTile != null)
            {
                ProcessTrees(OSMTile, satelliteTile);
                OSMTiles.Remove(OSMTile);
            } else
            {
                satelitesTiles.Add(satelliteTile);
            }
        }

        public void ProcessTrees(Map.Tile OSMtile, Map.Tile satelliteTile)
        {
            Color32[] pixels = satelliteTile.data.sprite.texture.GetPixels32();
            Color32[] mapPixels = OSMtile.data.sprite.texture.GetPixels32();

            Vector3 cellScale = mapManager.transform.localScale;

            Thread thread = new Thread(() => this.ProcessImage(
                satelliteTile.tilemapPosition,
                cellScale,
                pixels,
                mapPixels)
            );

            thread.Start();
        }

        public void ProcessImage(Vector3Int mapPosition, Vector3 tileScale, Color32[] pixels, Color32[] mapPixels)
        {
            try
            {
                System.Random rnd = new System.Random();

                for (int i = 0; i < pixels.Length; i++)
                {
                    int x = i % 256;
                    int y = i / 256;

                    // Drop the water trees
                    bool authorizedColor = authorizedColors.Exists(color => color.r == mapPixels[i].r && color.g == mapPixels[i].g && color.b == mapPixels[i].b);
                    //bool onAuthorizedColor = mapPixels[i].r == 170 && mapPixels[i].g == 211 && mapPixels[i].b == 223;

                    if (pixels[i].g - pixels[i].r > 10 && x%8 == 0 && y% 8 == 0 && authorizedColor)
                    {
                        Vector3 position = new Vector3(
                          earthManager.tilesManager.initialOffset.value.x * earthManager.tilesManager.tileSize.value + (mapPosition.x * tileScale.x * 256) + tileScale.x * x,
                          0,
                          earthManager.tilesManager.initialOffset.value.y * earthManager.tilesManager.tileSize.value + (mapPosition.y * tileScale.y * 256) + tileScale.y * y
                          );
                        trees.Enqueue(position);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public void AddTree(Vector3 position)
        {
            GameObject go = GameObject.Instantiate(tree);
            go.transform.position = position;
            go.AddComponent<FunkySheep.Earth.Components.SetHeight>();

            go.transform.localScale = new Vector3(
                UnityEngine.Random.Range(3, 5),
                UnityEngine.Random.Range(3, 5),
                UnityEngine.Random.Range(3, 5)
            );
            go.transform.Rotate(
                new Vector3(
                UnityEngine.Random.Range(0, 10),
                UnityEngine.Random.Range(0, 360),
                UnityEngine.Random.Range(0, 10)
                )
            );
            go.transform.parent = transform;
        }

        public void Create(int frameCount)
        {
            for (int i = 0; i < frameCount && i < trees.Count; i++)
            {
                Vector3 tree;
                if (trees.TryDequeue(out tree))
                {
                    AddTree(
                    new Vector3(
                        tree.x,
                        0,
                        tree.z
                    ));
                }
            }
        }
    }
}

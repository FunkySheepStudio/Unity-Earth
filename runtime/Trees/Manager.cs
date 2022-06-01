using System;
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
        public int drawDistance = 100;

        private void Update()
        {
            Create();
        }

        public void AddedTreeTile(Map.Tile tile)
        {
            Color32[] pixels = tile.data.sprite.texture.GetPixels32();
            Vector3 cellScale = mapManager.transform.localScale;

            Thread thread = new Thread(() => this.ProcessImage(
                tile.tilemapPosition,
                cellScale,
                pixels)
            );

            thread.Start();
        }

        public void ProcessImage(Vector3Int mapPosition, Vector3 tileScale, Color32[] pixels)
        {
            try
            {
                System.Random rnd = new System.Random();

                for (int i = 0; i < pixels.Length; i++)
                {
                    int x = i % 256;
                    int y = i / 256;

                    if (pixels[i].g - pixels[i].r > 10 && x%8 == 0 && y% 8 == 0)
                    {
                        Vector3 position = new Vector3(
                          earthManager.tilesManager.initialOffset.value.x * earthManager.tilesManager.tileSize.value + (mapPosition.x * tileScale.x * 256) + tileScale.x * x,
                          0,
                          earthManager.tilesManager.initialOffset.value.y * earthManager.tilesManager.tileSize.value + (mapPosition.y * tileScale.y * 256) + tileScale.y * y
                          );

                        if (x!=0 && y!=0)
                        {
                            int rndX = rnd.Next(-8, 8);
                            int rndY = rnd.Next(-8, 8);

                            position += new Vector3(
                                rndX,
                                0,
                                rndY
                                );
                        }

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

        public void Create()
        {
            for (int i = 0; i < trees.Count; i++)
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

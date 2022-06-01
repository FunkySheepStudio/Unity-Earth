using System;
using UnityEngine;

namespace FunkySheep.Earth.Roads
{
    public class Tile : FunkySheep.Tiles.Tile
    {
        public FunkySheep.Earth.Manager earthManager;
        Terrain.Tile terrainTile;
        float terrainResolution;
        byte[] osmFile;
        double[] gpsBoundaries;
        Vector2 worldBoundaryStart;
        Vector2 worldBoundaryEnd;
        float[,] heights;

        //public Graphs.Graph<Vector2> graph = new Graphs.Graph<Vector2>();
        public Tile(Vector2Int postition) : base(postition)
        {
        }

        public void SetBoudaries(double[] gpsBoundaries)
        {
            this.gpsBoundaries = gpsBoundaries;

            worldBoundaryStart = earthManager.CalculatePosition(this.gpsBoundaries[0], this.gpsBoundaries[1]);
            worldBoundaryEnd = earthManager.CalculatePosition(this.gpsBoundaries[2], this.gpsBoundaries[3]);
        }

        public void SetTerrainTile(Terrain.Tile terrainTile)
        {
            this.terrainTile = terrainTile;
            terrainResolution = terrainTile.terrain.terrainData.heightmapResolution;
            heights = new float
            [
              terrainTile.terrain.terrainData.heightmapResolution,
              terrainTile.terrain.terrainData.heightmapResolution
            ];

            Buffer.BlockCopy(
              terrainTile.heights,
              0,
              heights, 0,
              terrainTile.heights.Length * sizeof(float)
            );
        }

        public void SetOsmFile(byte[] osmFile)
        {
            this.osmFile = osmFile;
        }

        public void ExtractOsmData()
        {
            try
            {
                FunkySheep.OSM.Data parsedData = FunkySheep.OSM.Parser.Parse(osmFile);
                foreach (FunkySheep.OSM.Way way in parsedData.ways)
                {

                    for (int i = 1; i < way.nodes.Count; i++)
                    {
                        Node previousNode = new Node(
                          way.nodes[i - 1].latitude,
                          way.nodes[i - 1].longitude
                        );

                        Node node = new Node(
                          way.nodes[i].latitude,
                          way.nodes[i].longitude
                        );

                        ProcessSegment(way, previousNode, node);
                    }
                }

                // Trigger the terrain update
                terrainTile.heights = heights;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public void ProcessSegment(FunkySheep.OSM.Way way, Node previousNode, Node node)
        {
            string roadType = way.tags.Find(tag => tag.name == "highway").value;
            int roadSize = 1;

            switch (roadType)
            {
                case "primary":
                    roadSize = 3;
                    break;
                case "secondary":
                    roadSize = 2;
                    break;
                case "tertiary":
                    roadSize = 1;
                    break;
                default:
                    break;
            }

            previousNode.SetWorldPosition(earthManager);
            node.SetWorldPosition(earthManager);
            float step = earthManager.tilesManager.tileSize.value / terrainResolution;

            Vector2 previousNodeGridPosition = new Vector2(
              Mathf.RoundToInt(previousNode.worldPosition.x / step) * step,
              Mathf.RoundToInt(previousNode.worldPosition.y / step) * step
            );

            Vector2 nodeGridPosition = new Vector2(
              Mathf.RoundToInt(node.worldPosition.x / step) * step,
              Mathf.RoundToInt(node.worldPosition.y / step) * step
            );

            float diag = Diagolale(previousNodeGridPosition, nodeGridPosition);

            for (float i = 0; i < diag; i++)
            {
                // Interpolate a position in grid between 2 points
                // https://en.wikipedia.org/wiki/Line_drawing_algorithm#:~:text=In%20computer%20graphics%2C%20a%20line,rasterize%20lines%20in%20one%20color.
                float t = diag == 0 ? 0f : i / diag;
                Vector2 gridPoint = Vector2.Lerp(previousNodeGridPosition, nodeGridPosition, t);

                // round the point on the world grid
                gridPoint = new Vector2(
                  Mathf.RoundToInt(gridPoint.x / step) * step,
                  Mathf.RoundToInt(gridPoint.y / step) * step
                );

                // We check the point position inside the boudaries
                if (
                  gridPoint.x > worldBoundaryStart.x &&
                  gridPoint.y > worldBoundaryStart.y &&
                  gridPoint.x < worldBoundaryEnd.x &&
                  gridPoint.y < worldBoundaryEnd.y
                )
                {
                    // Get the inside tile position relative to the terrain resolution
                    gridPoint = earthManager.tilesManager.InsideTilePosition(gridPoint) * (terrainResolution - 1);
                    Vector2Int terrainCell = new Vector2Int(
                      Mathf.RoundToInt(gridPoint.y),
                      Mathf.RoundToInt(gridPoint.x)
                    );

                    for (int x = -roadSize; x <= roadSize; x++)
                    {
                        for (int y = -roadSize; y <= roadSize; y++)
                        {
                            if (terrainCell.x + x > (terrainResolution - 1) || terrainCell.x + x < 0)
                                break;
                            if (terrainCell.y + y > (terrainResolution - 1) || terrainCell.y + y < 0)
                                break;

                            heights[
                              terrainCell.x + x,
                              terrainCell.y + y
                            ] = terrainTile.heights[
                              terrainCell.x,
                              terrainCell.y
                            ];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the maximum diagonale
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public float Diagolale(Vector2 p0, Vector2 p1)
        {
            float dx = p1.x - p0.x;
            float dy = p1.y - p0.y;
            float diagonal = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
            return diagonal;
        }
    }
}

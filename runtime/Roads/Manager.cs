using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace FunkySheep.Earth.Roads
{
    public class Manager : MonoBehaviour
    {
        public FunkySheep.Types.String url;
        public FunkySheep.Types.Int32 terrainTextureResolution;
        public Earth.Manager earthManager;
        Queue<Tile> pendingTiles = new Queue<Tile>();

        private void Update()
        {
            if (pendingTiles.Count != 0)
            {
                Thread extractOsmThread = new Thread(() => pendingTiles.Dequeue().ExtractOsmData());
                extractOsmThread.Start();
            }
        }

        public void AddTile(Terrain.Tile terrainTile)
        {
            double[] gpsBoundaries = FunkySheep.Earth.Map.Utils.CaclulateGpsBoundaries(earthManager.zoomLevel.value, terrainTile.position);
            string interpolatedUrl = InterpolatedUrl(gpsBoundaries);

            StartCoroutine(FunkySheep.Network.Downloader.Download(interpolatedUrl, (fileID, file) =>
            {
                Tile roadTile = new Tile(terrainTile.position);
                roadTile.earthManager = earthManager;
                roadTile.SetBoudaries(gpsBoundaries);
                roadTile.SetTerrainTile(terrainTile);
                roadTile.SetOsmFile(file);
                pendingTiles.Enqueue(roadTile);
            }));
        }

        /// <summary>
        /// Interpolate the url inserting the boundaries and the types of OSM data to download
        /// </summary>
        /// <param boundaries="boundaries">The gps boundaries to download in</param>
        /// <returns>The interpolated Url</returns>
        public string InterpolatedUrl(double[] boundaries)
        {
            string[] parameters = new string[5];
            string[] parametersNames = new string[5];

            parameters[0] = boundaries[0].ToString().Replace(',', '.');
            parametersNames[0] = "startLatitude";

            parameters[1] = boundaries[1].ToString().Replace(',', '.');
            parametersNames[1] = "startLongitude";

            parameters[2] = boundaries[2].ToString().Replace(',', '.');
            parametersNames[2] = "endLatitude";

            parameters[3] = boundaries[3].ToString().Replace(',', '.');
            parametersNames[3] = "endLongitude";

            return url.Interpolate(parameters, parametersNames);
        }
    }
}



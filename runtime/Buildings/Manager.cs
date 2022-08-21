using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace FunkySheep.Earth.Buildings
{
    [AddComponentMenu("FunkySheep/Earth/Earth Buildings Manager")]
    public class Manager : MonoBehaviour
    {
        public FunkySheep.Types.String urlTemplate;
        public FunkySheep.Earth.Manager earthManager;
        public ConcurrentQueue<Building> buildings = new ConcurrentQueue<Building>();
        public Material floorMaterial;
        public FunkySheep.Events.GameObjectEvent onBuildingCreation;

        public GameObject buildingPrefab;

        public void DownLoad(Vector2Int position)
        {
            double[] gpsBoundaries = FunkySheep.Earth.Map.Utils.CaclulateGpsBoundaries(earthManager.zoomLevel.value, position);
            string interpolatedUrl = InterpolatedUrl(gpsBoundaries);
            StartCoroutine(FunkySheep.Network.Downloader.Download(interpolatedUrl, (fileID, file) =>
            {
                Thread extractOsmThread = new Thread(() => ExtractOsmData(file));
                extractOsmThread.Start();
            }));
        }

        private void Update()
        {
            Create(50);
        }

        public void ExtractOsmData(byte[] osmFile)
        {
            try
            {
                FunkySheep.OSM.Data parsedData = FunkySheep.OSM.Parser.Parse(osmFile);
                foreach (FunkySheep.OSM.Way way in parsedData.ways)
                {
                    Building building = new Building(way.id);

                    for (int i = 0; i < way.nodes.Count - 1; i++)
                    {
                        Vector2 point = earthManager.CalculatePosition(way.nodes[i].latitude, way.nodes[i].longitude);
                        building.points.Add(point);
                    }

                    building.tags = way.tags;

                    building.Initialize();
                    buildings.Enqueue(building);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
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

            return urlTemplate.Interpolate(parameters, parametersNames);
        }

        public void Create(int nbPerFrame)
        {
            for (int i = 0; i < nbPerFrame && i < buildings.Count; i++)
            {
                Building building;
                if (buildings.TryDequeue(out building))
                {
                    building.onBuildingCreation = onBuildingCreation;

                    Vector3 buildingPosition = new Vector3(
                      building.position.x,
                      0,
                      building.position.y
                    );

                    GameObject go;

                    if (buildingPrefab)
                    {
                        go = GameObject.Instantiate(buildingPrefab);
                        go.name = building.id.ToString();
                    } else {
                        go = new GameObject(building.id.ToString());
                    }
                     
                    go.tag = "Floor";
                    go.transform.position = buildingPosition;
                    go.transform.parent = transform;
                    FunkySheep.Earth.Buildings.Floor floor = go.AddComponent<FunkySheep.Earth.Buildings.Floor>();
                    FunkySheep.Earth.Components.SetHeight setHeight = go.AddComponent<FunkySheep.Earth.Components.SetHeight>();
                    floor.building = building;
                    floor.material = floorMaterial;

                    setHeight.action = floor.Create;
                }
            }
        }
    }
}

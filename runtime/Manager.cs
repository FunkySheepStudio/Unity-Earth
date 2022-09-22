using UnityEngine;

namespace FunkySheep.Earth
{
    [AddComponentMenu("FunkySheep/Earth/Earth Manager")]
    [RequireComponent(typeof(FunkySheep.Tiles.Manager))]
    public class Manager : MonoBehaviour
    {
        public FunkySheep.Types.Int32 zoomLevel;
        public FunkySheep.Types.Double initialLatitude;
        public FunkySheep.Types.Double initialLongitude;
        public FunkySheep.Types.Vector2 initialMapPosition;
        public FunkySheep.Types.Vector2 initialMercatorPosition;
        public FunkySheep.Events.SimpleEvent onStarted;
        public FunkySheep.Events.Vector2IntEvent onAddedTile;
        public FunkySheep.Tiles.Manager tilesManager;
        public bool AutoInit = false;

        private void Awake()
        {
            tilesManager = GetComponent<FunkySheep.Tiles.Manager>();
        }

        private void Start()
        {
            if (AutoInit)
                Init();
        }

        public void AddTile(Vector2Int gridPosition)
        {
            if (!tilesManager.Exist(gridPosition))
            {
                tilesManager.Add(gridPosition);

                // Invert the Y axis since osm is inverted
                Vector2Int calculatedMapPosition = new Vector2Int(
                  Mathf.FloorToInt(initialMapPosition.value.x) + gridPosition.x,
                  Mathf.FloorToInt(initialMapPosition.value.y) - gridPosition.y
                );

                onAddedTile.Raise(calculatedMapPosition);
            }
        }

        public void Init()
        {
            initialMercatorPosition.value = Utils.toCartesianVector2(initialLongitude.value, initialLatitude.value);
            tilesManager.tileSize.value = (float)Map.Utils.TileSize(zoomLevel.value, initialLatitude.value);
            initialMapPosition.value = Map.Utils.GpsToMapReal(zoomLevel.value, initialLatitude.value, initialLongitude.value);
            tilesManager.initialOffset.value = new Vector2
            (
              -(initialMapPosition.value.x - Mathf.Floor(initialMapPosition.value.x)),
              -1 + (initialMapPosition.value.y - Mathf.Floor(initialMapPosition.value.y))
            );


            Vector2Int tilePosition = tilesManager.TilePosition(Vector2.zero);

            Vector2Int insideTileQuarterPosition = tilesManager.InsideTileQuarterPosition(Vector2.zero);

            if (onStarted)
                onStarted.Raise();
        }

        public Vector2 CalculatePosition(double latitude, double longitude)
        {
            Vector2 position = Map.Utils.GpsToMapReal(
              zoomLevel.value,
              latitude,
              longitude,
              initialMapPosition.value
            );

            position.y = -position.y;
            position *= tilesManager.tileSize.value;

            return position;
        }

        /// <summary>
        /// Calculate the current world tile given a world position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector2Int CalculateTilePosition(Vector3 position)
        {
            return FunkySheep.Tiles.Utils.TilePosition(
              new Vector2(
                position.x,
                position.z
              ),
              tilesManager.tileSize.value,
              tilesManager.initialOffset.value
            );
        }

        /// Calculate the current inside quarter tile position given a world position
        public Vector2Int CalculateInsideTilePosition(Vector3 position)
        {
            return FunkySheep.Tiles.Utils.InsideTileQuarterPosition(
              new Vector2(
                transform.position.x,
                transform.position.z
              ),
              tilesManager.tileSize.value,
              tilesManager.initialOffset.value
            );
        }

        /// Calculate GPS coordinates of a world position
        public (double latitude, double longitude) CalculateGPSCoordinates (Vector3 position)
        {
            var calculatedGPS = FunkySheep.Earth.Utils.toGeoCoord(
                    new Vector2(
                        initialMercatorPosition.value.x + position.x / Mathf.Cos(Mathf.Deg2Rad * (float)initialLatitude.value),
                        initialMercatorPosition.value.y + position.z / Mathf.Cos(Mathf.Deg2Rad * (float)initialLatitude.value)
                        )
                );
            
            return calculatedGPS;
        }
    }
}

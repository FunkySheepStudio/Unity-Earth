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
        public FunkySheep.Events.Vector2IntEvent onMapPositionChanged;
        public FunkySheep.Tiles.Manager tilesManager;

        private void Awake()
        {
            tilesManager = GetComponent<FunkySheep.Tiles.Manager>();
        }

        private void Start()
        {
            Reset();
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

                onMapPositionChanged.Raise(calculatedMapPosition);
            }
        }

        public void Reset()
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

            /*AddTile(tilePosition);
            AddTile(tilePosition + insideTileQuarterPosition.y * Vector2Int.up);
            AddTile(tilePosition + insideTileQuarterPosition.x * Vector2Int.right);
            AddTile(tilePosition + insideTileQuarterPosition);*/
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
    }
}

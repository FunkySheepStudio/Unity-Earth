using UnityEngine;

namespace FunkySheep.Earth.Roads
{
    public class Node
    {
        double latitude;
        double longitude;
        public Vector2 gpsCoordinates;
        public Vector2 worldPosition;
        public Node(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            gpsCoordinates = new Vector2(
              (float)latitude,
              (float)longitude
            );
        }

        /// <summary>
        /// Check if the node gps coordinates are in the tile gps boundaries
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool IsInsideBoundaries(double[] gpsBoundaries)
        {
            if (latitude < gpsBoundaries[0])
                return false;
            if (longitude < gpsBoundaries[1])
                return false;
            if (latitude > gpsBoundaries[2])
                return false;
            if (longitude > gpsBoundaries[3])
                return false;

            return true;
        }

        public void SetWorldPosition(FunkySheep.Earth.Manager earthManager)
        {
            worldPosition = earthManager.CalculatePosition(gpsCoordinates.x, gpsCoordinates.y);
        }
    }
}
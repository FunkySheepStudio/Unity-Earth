using System;
using UnityEngine;

namespace FunkySheep.Earth.Map
{
    public static class Utils
    {
        /// <summary>
        /// Get the map tile position depending on zoom level and GPS postions
        /// </summary>
        /// <returns></returns>
        public static Vector2Int GpsToMap(int zoom, double latitude, double longitude)
        {
            return new Vector2Int(
                LongitudeToX(zoom, longitude),
                LatitudeToZ(zoom, latitude)
            );
        }

        /// <summary>
        /// Get the map tile position depending on zoom level and GPS postions
        /// https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Lon..2Flat._to_tile_numbers
        /// </summary>
        /// <returns></returns>
        public static Vector2 GpsToMapReal(int zoom, double latitude, double longitude)
        {
            Vector2 p = new Vector2();
            p.x = (float)((longitude + 180.0) / 360.0 * (1 << zoom));
            p.y = (float)((1.0 - Math.Log(Math.Tan(latitude * Math.PI / 180.0) +
              1.0 / Math.Cos(latitude * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

            return p;
        }

        /// <summary>
        /// Get the map tile position depending on zoom level and GPS postions
        /// https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Lon..2Flat._to_tile_numbers
        /// </summary>
        /// <returns></returns>
        public static Vector2 GpsToMapReal(int zoom, double latitude, double longitude, Vector2 offset)
        {
            Vector2 p = new Vector2();
            p.x = (float)(((longitude + 180.0) / 360.0 * (1 << zoom)) - offset.x);
            p.y = (float)(((1.0 - Math.Log(Math.Tan(latitude * Math.PI / 180.0) +
              1.0 / Math.Cos(latitude * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom)) - offset.y);

            return p;
        }

        /// <summary>
        /// Get the X number of the tile relative to Longitude position
        /// </summary>
        /// <returns></returns>
        public static int LongitudeToX(int zoom, double longitude)
        {
            return (int)(Math.Floor((longitude + 180.0) / 360.0 * (1 << zoom)));
        }

        /// <summary>
        /// Get the X number of the tile relative to Longitude position
        /// </summary>
        /// <returns></returns>
        public static float LongitudeToXReal(int zoom, double longitude)
        {
            return (float)((longitude + 180.0) / 360.0 * (1 << zoom));
        }


        /// <summary>
        /// Get the Real X number inside the tile
        /// </summary>
        /// <returns></returns>
        public static float LongitudeToInsideX(int zoom, double longitude)
        {
            return (float)((longitude + 180.0) / 360.0 * (1 << zoom) - LongitudeToX(zoom, longitude));
        }

        /// <summary>
        /// /// Get the Y number of the tile relative to Latitude position
        /// /// !!! The Y position is the reverse of the cartesian one !!!
        /// </summary>
        /// <returns></returns>
        public static int LatitudeToZ(int zoom, double latitude)
        {
            return (int)Math.Floor((1 - Math.Log(Math.Tan(Mathf.Deg2Rad * latitude) + 1 / Math.Cos(Mathf.Deg2Rad * latitude)) / Math.PI) / 2 * (1 << zoom));
        }

        /// <summary>
        /// /// Get the Y number of the tile relative to Latitude position
        /// /// !!! The Y position is the reverse of the cartesian one !!!
        /// </summary>
        /// <returns></returns>
        public static float LatitudeToZReal(int zoom, double latitude)
        {
            return (float)((1 - Math.Log(Math.Tan(Mathf.Deg2Rad * latitude)) / Math.PI) / 2 * (1 << zoom));
        }

        /// <summary>
        /// /// Get the Real Y number inside of the tile
        /// </summary>
        /// <returns></returns>
        public static float LatitudeToInsideZ(int zoom, double latitude)
        {
            return (float)((1 - Math.Log(Math.Tan(Mathf.Deg2Rad * latitude) + 1 / Math.Cos(Mathf.Deg2Rad * latitude)) / Math.PI) / 2 * (1 << zoom)) - LatitudeToZ(zoom, latitude);
        }

        /// <summary>
        /// Get the Longitude of the tile relative to X position
        /// </summary>
        /// <returns></returns>
        public static double tileX2long(int zoom, float xPosition)
        {
            return xPosition / (double)(1 << zoom) * 360.0 - 180;
        }

        /// <summary>
        ///  Get the latitude of the tile relative to Y position
        /// </summary>
        /// <returns></returns>
        public static double tileZ2lat(int zoom, float zposition)
        {
            double n = Math.PI - 2.0 * Math.PI * zposition / (double)(1 << zoom);
            return 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
        }

        /// <summary>
        /// Calculate size of the OSM tile depending on the zoomValue level and latitude
        /// </summary>
        /// <returns></returns>
        public static double TileSize(int zoom, double latitude)
        {
            return 156543.03 / Math.Pow(2, zoom) * Math.Cos(Mathf.Deg2Rad * latitude) * 256;
        }

        /// <summary>
        /// Calculate size of the OSM tile depending on the zoomValue level.
        /// </summary>
        /// <returns></returns>
        public static double TileSize(int zoom)
        {
            return 156543.03 / Math.Pow(2, zoom) * 256;
        }

        /// <summary>
        /// Calculate the GPS boundaries of a tile depending on zoom size
        /// </summary>
        /// <returns>A Double[4] containing [StartLatitude, StartLongitude, EndLatitude, EndLongitude]</returns>
        public static Double[] CaclulateGpsBoundaries(int zoom, double latitude, double longitude)
        {
            Vector2Int mapPosition = GpsToMap(zoom, latitude, longitude);

            double startlatitude = Utils.tileZ2lat(zoom, mapPosition.y + 1);
            double startlongitude = Utils.tileX2long(zoom, mapPosition.x);
            double endLatitude = Utils.tileZ2lat(zoom, mapPosition.y);
            double endLongitude = Utils.tileX2long(zoom, mapPosition.x + 1);

            Double[] boundaries = new Double[4];

            boundaries[0] = startlatitude;
            boundaries[1] = startlongitude;
            boundaries[2] = endLatitude;
            boundaries[3] = endLongitude;

            return boundaries;
        }

        /// <summary>
        /// Calculate the GPS boundaries of a tile depending on zoom size and the position on the map
        /// </summary>
        /// <returns>A Double[4] containing [StartLatitude, StartLongitude, EndLatitude, EndLongitude]</returns>
        public static Double[] CaclulateGpsBoundaries(int zoom, Vector2Int mapPosition)
        {
            double startlatitude = Utils.tileZ2lat(zoom, mapPosition.y + 1);
            double startlongitude = Utils.tileX2long(zoom, mapPosition.x);
            double endLatitude = Utils.tileZ2lat(zoom, mapPosition.y);
            double endLongitude = Utils.tileX2long(zoom, mapPosition.x + 1);

            Double[] boundaries = new Double[4];

            boundaries[0] = startlatitude;
            boundaries[1] = startlongitude;
            boundaries[2] = endLatitude;
            boundaries[3] = endLongitude;

            return boundaries;
        }

        /// <summary>
        /// Convert from color channel values in 0.0-1.0 range to elevation in meters:
        /// 21768
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static float ColorToElevation(Color color)
        {
            float height = (Mathf.Floor(color.r * 256.0f) * 256.0f + Mathf.Floor(color.g * 256.0f) + color.b) - 32768.0f;
            return height;
        }
    }
}

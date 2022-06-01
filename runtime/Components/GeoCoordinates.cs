using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FunkySheep.Earth.Components
{
    [AddComponentMenu("FunkySheep/Earth/Components/GeoCoordinates")]
    public class GeoCoordinates : MonoBehaviour
    {
        public FunkySheep.Earth.Manager earth;
        public double latitude;
        public double longitude;
        // Update is called once per frame
        void Update()
        {
            var calculatedGPS = FunkySheep.Earth.Utils.toGeoCoord(
                        new Vector2(
                            earth.initialMercatorPosition.value.x + transform.position.x / Mathf.Cos(Mathf.Deg2Rad * (float)earth.initialLatitude.value),
                            earth.initialMercatorPosition.value.y + transform.position.z / Mathf.Cos(Mathf.Deg2Rad * (float)earth.initialLatitude.value)
                            )
                    );
            latitude = calculatedGPS.latitude;
            longitude = calculatedGPS.longitude;
        }
    }

}

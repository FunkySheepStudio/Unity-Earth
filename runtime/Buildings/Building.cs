using System;
using System.Collections.Generic;
using UnityEngine;

namespace FunkySheep.Earth.Buildings
{
    [Serializable]
    public class Building
    {
        public long id;
        public List<Vector2> points = new List<Vector2>();
        public List<Vector2> innerPoints = new List<Vector2>();
        public int stagesCount = 5;
        public Vector2 position;
        public float area;
        public float? lowPoint = null;
        public float? hightPoint = null;
        public List<OSM.Tag> tags = new List<OSM.Tag>();
        public FunkySheep.Events.GameObjectEvent onBuildingCreation;

        public Building(long id)
        {
            this.id = id;
        }

        public void Initialize()
        {
            this.position = Position();
            SetFirstPoint();
            SetClockWise();
            area = Area();
        }

        /// <summary>
        /// Calculate the center relative to the points
        /// </summary>
        /// <returns>The center of all points</returns>
        public Vector2 Position()
        {
            // Calculate the center
            Vector2 center = Vector2.zero;
            for (int i = 0; i < points.Count; i++)
            {
                center += points[i];
            }

            center /= points.Count;

            return center;
        }

        /// <summary>
        /// Calculate the building area
        /// </summary>
        /// <returns></returns>
        public float Area()
        {
            float area = 0;

            for (int i = 0; i < points.Count; i++)
            {
                area += Vector2.Distance(points[i], points[(i + 1) % points.Count]);
            }

            return area;
        }

        /// <summary>
        /// If the Vector Array is clockwise, return it
        /// </summary>
        /// <returns></returns>
        public void SetClockWise()
        {
            int result = FunkySheep.Vectors.Utils.IsClockWise(points[1], points[points.Count - 1], points[0]);
            if (result < 0)
            {
                points.Reverse();
            }
        }

        /// <summary>
        /// Set the first point of the building (the farest from the center)
        /// </summary>
        public void SetFirstPoint()
        {
            int maxPointIndex = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if ((points[maxPointIndex] - position).magnitude < (points[i] - position).magnitude)
                {
                    maxPointIndex = i;
                }
            }

            Vector2[] tempPoints = new Vector2[points.Count];
            points.CopyTo(tempPoints);

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = tempPoints[(i + maxPointIndex) % points.Count];
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace FunkySheep.Earth.Buildings
{
    [RequireComponent(typeof(ProBuilderMesh))]
    [RequireComponent(typeof(MeshCollider))]
    public class Stage : MonoBehaviour
    {
        public Floor floor;
        ProBuilderMesh mesh;

        private void Awake()
        {
            mesh = this.GetComponent<ProBuilderMesh>();
        }

        public void Create()
        {
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < floor.building.points.Count; i++)
            {
                points.Add(new Vector3(
                  floor.building.points[i].x - floor.building.position.x,
                  0,
                  floor.building.points[i].y - floor.building.position.y
                ));
            }

            mesh.CreateShapeFromPolygon(points, 0.5f, false);
            GetComponent<MeshRenderer>().material = floor.material;
            CreateWalls();
        }

        void CreateWalls()
        {
            for (int i = 0; i < floor.building.points.Count; i++)
            {
                int iA = i;
                int iB = (i + 1) % floor.building.points.Count;

                Vector3 pointA = new Vector3(
                  floor.building.points[iA].x - floor.building.position.x,
                  0,
                  floor.building.points[iA].y - floor.building.position.y
                );

                Vector3 pointAInner = new Vector3(
                  floor.building.innerPoints[iA].x - floor.building.position.x,
                  0,
                  floor.building.innerPoints[iA].y - floor.building.position.y
                );

                Vector3 pointB = new Vector3(
                  floor.building.points[iB].x - floor.building.position.x,
                  0,
                  floor.building.points[iB].y - floor.building.position.y
                );

                Vector3 pointBInner = new Vector3(
                  floor.building.innerPoints[iB].x - floor.building.position.x,
                  0,
                  floor.building.innerPoints[iB].y - floor.building.position.y
                );

                GameObject go = new GameObject(i.ToString());
                go.transform.position = transform.position;
                go.transform.parent = transform;
                Wall wallComponent = go.AddComponent<Wall>();
                wallComponent.floor = floor;

                wallComponent.points.Add(pointA);
                wallComponent.points.Add(pointB);
                wallComponent.points.Add(pointBInner);
                wallComponent.points.Add(pointAInner);

                wallComponent.Create();
            }
        }
    }
}

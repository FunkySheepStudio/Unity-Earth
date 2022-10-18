using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace FunkySheep.Earth.Buildings
{
    public class Floor : MonoBehaviour
    {
        public Building building;
        public Material material;
        ProBuilderMesh mesh;
        public Vector3[] newPositions;

        public void Create()
        {
           
            // Get the min and max heights
            foreach (Vector2 point in building.points)
            {
                float? height = Terrain.Manager.GetHeight(point);

                if (height == null)
                {
                    return;
                }


                if (building.lowPoint == null || height.Value <= building.lowPoint)
                {
                    building.lowPoint = height.Value;
                }

                if (building.hightPoint == null || height.Value >= building.hightPoint)
                {
                    building.hightPoint = height.Value;
                }
            }

            gameObject.AddComponent<ProBuilderMesh>();
            gameObject.AddComponent<MeshCollider>();
            mesh = this.GetComponent<ProBuilderMesh>();

            // Create the shape
            newPositions = new Vector3[building.points.Count];
            for (int i = 0; i < newPositions.Length; i++)
            {
                newPositions[i].x = building.points[i].x - building.position.x;
                newPositions[i].y = 0;
                newPositions[i].z = building.points[i].y - building.position.y;
            }

            mesh.CreateShapeFromPolygon(newPositions, building.hightPoint.Value - building.lowPoint.Value + 0.2f, false);

            foreach (var face in mesh.faces)
            {
                face.uv = new AutoUnwrapSettings()
                {
                    anchor = AutoUnwrapSettings.Anchor.LowerLeft,
                    scale = new Vector3(1f, 1f, 1f),
                    fill = AutoUnwrapSettings.Fill.Fit
                };
            }

            mesh.Refresh(RefreshMask.UV);

            GetComponent<MeshRenderer>().material = material;
            transform.position = new Vector3(building.position.x, building.lowPoint.Value, building.position.y);

            building.onBuildingCreation.Raise(gameObject);
        }

        public void GetInnerPoints()
        {
            building.innerPoints = new List<Vector2>(building.points);
            for (int i = 0; i < building.points.Count; i++)
            {
                int iA = i;
                int iB = (i + 1) % building.points.Count;
                int iC = (i + 2) % building.points.Count;

                Vector2 pointBprime =
                  building.points[iB] +
                    (
                      (building.points[iA] - building.points[iB]).normalized +
                      (building.points[iC] - building.points[iB]).normalized
                    ).normalized * 0.5f;

                Vector3 pointBprime3D = new Vector3(
                  pointBprime.x,
                  building.hightPoint.Value + 1f,
                  pointBprime.y
                );

                // Call Raycast
                RaycastHit hit;
                Physics.Raycast(pointBprime3D, Vector3.down, out hit, 1.2f);
                if (!(hit.transform == transform))
                {
                    pointBprime =
                    building.points[iB] -
                      (
                        (building.points[iA] - building.points[iB]).normalized +
                        (building.points[iC] - building.points[iB]).normalized
                      ).normalized * 0.5f;

                    pointBprime3D = new Vector3(
                      pointBprime.x,
                      building.hightPoint.Value + 1f,
                      pointBprime.y
                    );
                }

                building.innerPoints[iB] = pointBprime;
            }
        }
    }
}

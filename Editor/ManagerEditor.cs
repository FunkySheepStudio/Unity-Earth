#if (UNITY_EDITOR) 

using UnityEngine;
using UnityEditor;

namespace FunkySheep.Earth
{
    [CustomEditor(typeof(Manager))]
    public class ManagerEditor : Editor
    {
        Vector2Int tileToBeAdded;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Manager myScript = (Manager)target;

            tileToBeAdded = EditorGUILayout.Vector2IntField("Added Tile position", tileToBeAdded);

            if (GUILayout.Button("Add a tile"))
            {
                myScript.AddTile(tileToBeAdded);
            }
        }
    }

}
#endif

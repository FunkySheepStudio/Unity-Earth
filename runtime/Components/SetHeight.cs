using System;
using UnityEngine;


namespace FunkySheep.Earth.Components
{
    [AddComponentMenu("FunkySheep/Earth/Components/Set Height")]
    public class SetHeight : MonoBehaviour
    {
        public float offset = 0;
        public Action action = null;
        void Update()
        {
           float? height = Terrain.Manager.GetHeight(transform.position);
            if (height != null)
            {
                transform.position += Vector3.up * (height.Value + offset);
                if (action != null)
                {
                    action.Invoke();
                }
                Destroy(this);
            }
        }
    }
}

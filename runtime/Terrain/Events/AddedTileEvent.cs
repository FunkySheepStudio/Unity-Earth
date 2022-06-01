using UnityEngine;

namespace FunkySheep.Earth.Terrain
{
    [CreateAssetMenu(menuName = "FunkySheep/Terrain/Events/AddedTile")]
    public class AddedTileEvent : FunkySheep.Events.Event<Tile>
    {
    }
}
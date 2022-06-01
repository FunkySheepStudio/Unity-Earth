using UnityEngine;

namespace FunkySheep.Earth.Map
{
    [CreateAssetMenu(menuName = "FunkySheep/Map/Events/AddedTile")]
    public class AddedTileEvent : FunkySheep.Events.Event<Tile>
    {
    }
}
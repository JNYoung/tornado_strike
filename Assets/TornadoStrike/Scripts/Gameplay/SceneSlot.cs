using UnityEngine;

namespace TornadoStrike.Gameplay
{
    public enum SceneSlotKind
    {
        StandardBuilding,
        VehicleSpawn,
        PowerPlant,
        PoliceStation,
        FireStation,
        Landmark,
        RainforestFuture
    }

    [DisallowMultipleComponent]
    public sealed class SceneSlot : MonoBehaviour
    {
        public string slotId = "city_slot";
        public SceneSlotKind kind = SceneSlotKind.StandardBuilding;
        public string displayNameKey = "slot_generic";
        public int recommendedTier = 1;
        public bool participatesInMvp = true;
    }
}

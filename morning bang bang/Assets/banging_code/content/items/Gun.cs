using banging_code.bullets;
using banging_code.player_logic;
using banging_code.runs_system;
using MothDIed;
using UnityEngine;

namespace banging_code.items
{
    [CreateAssetMenu(menuName = "Items/Gun")]
    public class Gun : MainItem
    {
        [field: SerializeField] public Bullet BulletPrefab { get; private set; }
        public override string ID => "gun";


        public override void PutOnPlayerInstance(PlayerRoot playerRoot)
        {
            Game.G<RunSystem>().Data.Level.PlayerInstance.Systems.Get<PlayerHands>().EnableItem<GunInstance>();
        }

        public override void OnDropped(ItemPickUp itemPickUp)
        {
            Game.G<RunSystem>().Data.Level.PlayerInstance.Systems.Get<PlayerHands>().DisableItem<GunInstance>();
        }
    }
}
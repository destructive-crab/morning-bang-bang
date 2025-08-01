using banging_code.common.extensions;
using banging_code.common;
using banging_code.interactions;
using banging_code.items;
using banging_code.player_logic.standard;
using banging_code.player_logic.states;
using DragonBones;
using MothDIed;
using MothDIed.InputsHandling;
using UnityEngine;

namespace banging_code.player_logic.rat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class RatPlayer : PlayerRoot
    {
        public Transform sideRoot;
        public Transform upRoot;
        public Transform downRoot;

        public override ID EntityID { get; } = new("player", true);

        protected override void InitializeComponents()
        {
            //physics
            CachedComponents.Register(GetComponent<Rigidbody2D>());
            CachedComponents.Register(GetComponent<Collider2D>());

            //visuals
            CachedComponents.Register(GetComponentInChildren<SpriteRenderer>());

            //interactions
            CachedComponents.Register(GetComponentInChildren<InteractorFoV>());
        }

        protected override void InitializeExtensions()
        {
            //visuals
            Systems.AddSystem(new PlayerAnimator());
            Systems.AddSystem(new Flipper(true));
            Systems.AddSystem(new VelocityFlipper());
            
            //addons
            Systems.AddSystem(new Interactor());
            var hands = new PlayerHands();

            hands.SetSideRoot(sideRoot);
            hands.SetUpRoot(upRoot);
            hands.SetDownRoot(downRoot);
            
            Systems.AddSystem(hands);
        }

        protected override void InitializeStates()
        {
            OverrideFactory<PlayerIdle>(new StandardStateFactory<StandardPlayerIdle>());
            OverrideFactory<PlayerMove>(new StandardStateFactory<StandardPlayerMove>());
        }

        protected override void FinishInitialization()
        {
            Systems.StartContainer();
            Game.RunSystem.Data.Inventory.SetMainItem(Game.RunSystem.Data.ItemsPool.GetNew("gun") as MainItem);
            
            EnterState(GetFactory<PlayerIdle>().GetState());
        }

        protected override void UpdateInheritor()
        {
            if (InputService.Movement != Vector2.zero)
            {
                EnterState(GetFactory<PlayerMove>().GetState());
            }
            else
            {
                EnterState(GetFactory<PlayerIdle>().GetState());
            }
        }

        public override void SetDirection(GameDirection direction)
        {
            base.SetDirection(direction);

            Systems.Get<PlayerHands>().RotateTo(direction);
        }
    }
}
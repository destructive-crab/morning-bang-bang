using banging_code.common.extensions;
using banging_code.common;
using banging_code.interactions;
using banging_code.player_logic.standard;
using banging_code.player_logic.states;
using MothDIed;
using MothDIed.InputsHandling;
using UnityEngine;

namespace banging_code.player_logic.rat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class RatPlayer : PlayerRoot
    {
        protected override void InitializeComponents()
        {
            //physics
            CachedComponents.Register(GetComponent<Rigidbody2D>());
            CachedComponents.Register(GetComponent<Collider2D>());

            //visuals
            CachedComponents.Register(GetComponentInChildren<Animator>());
            CachedComponents.Register(GetComponentInChildren<SpriteRenderer>());

            //interactions
            CachedComponents.Register(GetComponentInChildren<InteractorFoV>());
        }

        protected override void InitializeExtensions()
        {
            //visuals
            Extensions.AddExtension(new PlayerAnimator());
            Extensions.AddExtension(new Flipper(true));
            Extensions.AddExtension(new VelocityFlipper());
            
            //addons
            Extensions.AddExtension(new Interactor());
            Extensions.AddExtension(new PlayerHands(transform));
        }

        protected override void InitializeStates()
        {
            OverrideFactory<PlayerIdle>(new StandardStateFactory<StandardPlayerIdle>());
            OverrideFactory<PlayerMove>(new StandardStateFactory<StandardPlayerMove>());
        }

        protected override void FinishInitialization()
        {
            Extensions.StartContainer();
            Game.RunSystem.Data.Inventory.SetMainItem(null);
            
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
    }
}
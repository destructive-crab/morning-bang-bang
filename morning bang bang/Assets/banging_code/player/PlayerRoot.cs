using System.Collections.Generic;
using System;
using banging_code.common;
using banging_code.player_logic.states;
using MothDIed;
using UnityEngine;

namespace banging_code.player_logic
{
    public abstract class PlayerRoot : DepressedBehaviour
    {
        public PlayerState CurrentState { get; private set; }
        public GameDirection Direction;

        public PlayerIdle Idle => CurrentState as PlayerIdle;
        public PlayerMove Moving => CurrentState as PlayerMove;
        public PlayerRoll Rolling => CurrentState as PlayerRoll;
        
        public PlayerHands Hands { get; set; }

        private readonly Dictionary<Type, PlayerStateFactory> stateFactories = new Dictionary<Type, PlayerStateFactory>();
        
        protected abstract void InitializeComponents();
        protected virtual void InitializeExtensions() { }
        
        /// <summary> Use OverrideState<TState>() to add new state factory.</summary>
        protected abstract void InitializeStates();
        protected abstract void FinishInitialization();

        protected virtual void UpdateInheritor() {}
        protected virtual void FixedUpdateInheritor() {}
        protected virtual void OnEnableInheritor() {}
        protected virtual void OnDisableInheritor() {}

        private void OnEnable()
        {
            CurrentState?.Enter(this);
            Extensions.EnableAll();
            
            OnEnableInheritor();
        }

        private void OnDisable()
        {
            CurrentState?.Exit(this);
            Extensions.DisableAll();
            
            OnDisableInheritor();
        }

        private void Start()
        {
            Extensions.SetOwner(this);

            CachedComponents.Register<PlayerRoot>(this);
            
            InitializeComponents();
            
            InitializeExtensions();
            
            InitializeStates();
            
            FinishInitialization();
        }

        private void Update()
        {
            CurrentState?.Update(this);
            Extensions.UpdateContainer();
            
            UpdateInheritor();
        }

        private void FixedUpdate()
        {
            CurrentState?.FixedUpdate(this);
            Extensions.FixedUpdateContainer();
            
            FixedUpdateInheritor();
        }

        protected PlayerStateFactory GetFactory<TFactory>()
            where TFactory : PlayerState
        {
            return stateFactories[typeof(TFactory)]; 
        }

        public void OverrideFactory<TFactory>(PlayerStateFactory factory)
            where TFactory : PlayerState
        {
            if(stateFactories.ContainsKey(typeof(TFactory)))
            {
                stateFactories[typeof(TFactory)] = factory;
            }
            else
            {
                stateFactories.Add(typeof(TFactory), factory);   
            }
        }

        public TState Get<TState>()
            where TState : PlayerState
        {
            return CurrentState as TState;
        }

        public bool TryGet<TState>(out TState state)
            where TState : PlayerState
        {
            state = Get<TState>();
            return state != null;
        }
        
        public void EnterState(PlayerState state)
        {
            if (IsAvailable(state))
            {
                CurrentState?.Exit(this);
                CurrentState = state;
                
                Game.DIKernel.InjectWithBaseAnd(state, CachedComponents, Extensions);
                
                CurrentState.Enter(this);
            }
        }
        
        private bool IsAvailable(PlayerState state)
            => state != null && (CurrentState == null || (CurrentState != null && CurrentState.CanEnterFrom(state.GetType())));

        public void Activate()
        {
            for (var i = 0; i < Game.RunSystem.Data.Inventory.PassiveItems.Length; i++)
            {
                var item = Game.RunSystem.Data.Inventory.PassiveItems[i];
                item.ApplyToPlayerInstance(this);
            }

            var mainItem = Game.RunSystem.Data.Inventory.Main;
            
            if(mainItem != null)
            {
                mainItem.PutOnPlayerInstance(this);
            }
        }
    }
}
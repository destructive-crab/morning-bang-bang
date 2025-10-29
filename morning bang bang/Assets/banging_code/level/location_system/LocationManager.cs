using System;
using System.Collections.Generic;
using banging_code.common;
using banging_code.debug;
using MothDIed;
using MothDIed.Core.GameObjects;
using MothDIed.Scenes.SceneModules;
using UnityEngine;
using LGR = banging_code.debug.LGR;

namespace banging_code.level.entity_locating
{
    public sealed class LocationManager : SceneModule
    {
        private readonly Dictionary<ID, List<MonoEntity>> locations = new();
        private readonly Dictionary<MonoEntity, ID> entityToLocation = new();
        private readonly List<MonoEntity> allEntities = new();
        private readonly List<ID> allLocations = new();

        public event Action<MonoEntity, ID> OnEntitySwitchLocation;
        public event Action<ID> OnLocationEntitiesChanged;

        public LocationManager(LevelScene scene)
        {
            scene.Modules.AddModule(new LocationManagerFabricHook(this));
        }

        public MonoEntity[] GetEntitesFrom(ID location) => locations[location].ToArray();
        public ID GetLocationOf(MonoEntity entity) => entityToLocation[entity];

        public void RegisterLocation(ID location)
        {
            if (locations.ContainsKey(location))
            {
                LGR.PERR($"[LOCATION MANAGER: REGISTER] LOCATION ALREADY IN LIST {location.Get()}");
                return;
            }
            allLocations.Add(location);
            locations.Add(location, new List<MonoEntity>());
        }

        public void AddEntity(MonoEntity entity, ID location)
        {
            if(!ProcessEntity(entity) || !ProcessLocation(location)) return;
            
            allEntities.Add(entity);
            entityToLocation.Add(entity, location);
            locations[location].Add(entity);
            
            OnEntitySwitchLocation?.Invoke(entity, location);
        }

        public void RemoveEntity(MonoEntity entity)
        {
            if(!ProcessEntity(entity)) return;
            
            locations[entityToLocation[entity]].Remove(entity);
            entityToLocation.Remove(entity);
            allEntities.Remove(entity);
        }

        public bool TryRemove(MonoEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (!allEntities.Contains(entity))
            {
                return false;
            }
            
            locations[entityToLocation[entity]].Remove(entity);
            entityToLocation.Remove(entity);
            allEntities.Remove(entity);
            return true;
        }

        public void ChangeLocationOf(MonoEntity entity, ID newLocation)
        {
            if(entity!=null && !ProcessLocation(newLocation)) return;
            
            if(!allEntities.Contains(entity))
            {
                allEntities.Add(entity);
                entityToLocation.Add(entity, newLocation);
                locations[newLocation].Add(entity);               
            }
            else
            {
                var oldLocation = GetLocationOf(entity);
                locations[GetLocationOf(entity)].Remove(entity);
                locations[newLocation].Add(entity);               
                entityToLocation[entity] = newLocation;
                OnLocationEntitiesChanged?.Invoke(oldLocation);
            }
            
            OnEntitySwitchLocation?.Invoke(entity, newLocation);
            OnLocationEntitiesChanged?.Invoke(newLocation);
        }

        public void RemoveLocation(ID location)
        {
            if (!ProcessLocation(location)) return;

            if(GetEntitesFrom(location).Length > 0)
            {
#if UNITY_EDITOR
                LGR.PERR($"[LOCATION MANAGER : REMOVE] {location} STILL CONTAINS ENTITIES");
#endif
                return;
            }

            foreach (var entity in locations[location])
            {
                entityToLocation.Remove(entity);
            }

            locations.Remove(location);
            allLocations.Remove(location);
        }

        private bool ProcessEntity(MonoEntity entity)
        {
            if (entity == null)
            {
                LGR.PERR("[LOCATION MANAGER] GIVEN ENTITY IS NULL");
                return false;
            }

            return true;
        }

        private bool ProcessLocation(ID location)
        {
            return allLocations.Contains(location);
        }
    }

    public sealed class LocationManagerFabricHook : GameFabricSceneModule
    {
        private readonly LocationManager locationManager;

        public LocationManagerFabricHook(LocationManager locationManager)
        {
            this.locationManager = locationManager;
        }
        
        public override void BeforeGameObjectDestroyed(GameObject instance)
        {
            base.BeforeGameObjectDestroyed(instance);

            var entities = instance.GetComponentsInChildren<MonoEntity>(true);
            
            foreach (var monoEntity in entities)
            {
                locationManager.TryRemove(monoEntity);
            }
        }
    }
}
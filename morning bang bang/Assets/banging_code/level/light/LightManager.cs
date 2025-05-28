using System.Collections.Generic;
using banging_code.common;
using banging_code.common.rooms;
using MothDIed;
using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;

namespace banging_code.level.light
{
    public class LightManager : SceneModule
    {
        private Dictionary<ID, List<IControllableLight>> lights = new();
        private ID corridorID = new("corridor");

        public override void StartModule(Scene scene)
        {
            base.StartModule(scene);
            CollectAllLightsFromLevel();
            
            TurnOn(corridorID);
        }

        public void RegisterLightsAs(ID key, IControllableLight[] lights)
        {
            this.lights.TryAdd(key, new List<IControllableLight>());
            
            this.lights[key].AddRange(lights);
        }

        public void CollectAllLightsFromLevel()
        {
            foreach (Room room in Game.RunSystem.Data.Level.Hierarchy.Rooms)
            {
                var lights = room.GetComponentsInChildren<IControllableLight>();

                if (room is Corridor || room is StartRoom || room is FinalRoom)
                {
                    RegisterLightsAs(corridorID, lights);
                }
                RegisterLightsAs(room.RoomID, lights);
            }
        }

        public void TurnOn(ID key)
        {
            if (lights.ContainsKey(key))
            {
                foreach (var light in lights[key])
                {
                    light.TurnOn();
                }
            }
        }

        public void TurnOff(ID key)
        {
            if (lights.ContainsKey(key))
            {
                foreach (var light in lights[key])
                {
                    light.TurnOff();
                }
            }
        }
    }
}
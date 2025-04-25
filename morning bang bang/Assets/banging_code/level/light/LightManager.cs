using System.Collections.Generic;
using banging_code.common.rooms;
using MothDIed;
using MothDIed.DI;
using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;

namespace banging_code.level.light
{
    public class LightManager : SceneModule
    {
        private Dictionary<string, List<IControllableLight>> lights = new();

        public override void StartModule(Scene scene)
        {
            base.StartModule(scene);
            CollectAllLightsFromLevel();

            foreach (var key in lights.Keys)
            {
                TurnOff(key);
            }
            
            TurnOn("corridor");
        }

        public void RegisterLightsAs(string key, IControllableLight[] lights)
        {
            this.lights.TryAdd(key, new List<IControllableLight>());
            
            this.lights[key].AddRange(lights);
        }

        public void CollectAllLightsFromLevel()
        {
            foreach (Room room in Game.RunSystem.Data.Level.Hierarchy.Rooms)
            {
                var lights = room.GetComponentsInChildren<IControllableLight>();

                if (room is Corridor)
                {
                    RegisterLightsAs("corridor", lights);
                }
                RegisterLightsAs(room.ID, lights);
            }
        }

        public void TurnOn(string key)
        {
            if (lights.ContainsKey(key))
            {
                foreach (var light in lights[key])
                {
                    light.TurnOn();
                }
            }
        }

        public void TurnOff(string key)
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
using banging_code.ai.management;
using MothDIed;
using UnityEngine;

namespace banging_code.ai
{
    public class EntitySpawner : MonoBehaviour
    {
        public Entity bastardPrefab;

        public Entity[] Spawn()
        {
            var spawnPoints = transform.GetComponentsInChildren<EntitySpawnPoint>();

            
            foreach (var point in spawnPoints)
            {
                Game.CurrentScene.Fabric.Instantiate(bastardPrefab, point.transform.position, transform);
            }

            return null;
        }
    }
}
using banging_code.ai.management;
using MothDIed;
using UnityEngine;
using System.Collections.Generic;

namespace banging_code.ai
{
    public class EntitySpawner : MonoBehaviour
    {
        public Enemy bastardPrefab;

        public Enemy[] Spawn()
        {
            List<Enemy> enemies = new List<Enemy>();
            var spawnPoints = transform.GetComponentsInChildren<EntitySpawnPoint>();
            
            foreach (var point in spawnPoints)
            {
                enemies.Add(Game.CurrentScene.Fabric.Instantiate(bastardPrefab, point.transform.position, transform));
            }

            return enemies.ToArray();
        }
    }
}
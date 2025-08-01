using System.Collections.Generic;
using banging_code.ai;
using banging_code.ai.management;
using MothDIed;

public class ShowcasePuppySpawner : EntitySpawner
{
    public Puppy PuppyPrefab;
    
    public override MonoEntity[] Spawn()
    {
        var spawnPoints = transform.GetComponentsInChildren<EntitySpawnPoint>();

        List<MonoEntity> spawned = new();
        
        foreach (var entitySpawnPoint in spawnPoints)
        {
            spawned.Add(Game.SceneSwitcher.CurrentScene.Fabric.Instantiate(PuppyPrefab, entitySpawnPoint.transform.position, entitySpawnPoint.transform));
        }

        return spawned.ToArray();
    }
}
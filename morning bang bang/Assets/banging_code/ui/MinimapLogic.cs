using MothDIed;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MinimapLogic : MonoBehaviour
{
    public Material notURPMaterial;
    
    private void Start()
    {
        CreateMinimapLevelCopy();
    }

    private void CreateMinimapLevelCopy()
    {
        var minimapGrid = GameObject.Instantiate(Game.RunSystem.Data.Level.Hierarchy.LevelGrid);
        minimapGrid.name = "MINIMAP_LEVEL_COPY";

        foreach (Transform gridChild in minimapGrid.transform)
        {
            gridChild.gameObject.layer = LayerMask.NameToLayer("Minimap");
        }

        foreach (var tilemap in minimapGrid.GetComponentsInChildren<TilemapRenderer>())
        {
            tilemap.material = notURPMaterial;
        }

        Destroy(minimapGrid.GetComponentInChildren<TilemapCollider2D>());
    }

    private void Update()
    {
        if (Game.RunSystem.IsInRun)
        {
            transform.position = new Vector3(Game.RunSystem.Data.Level.PlayerInstance.transform.position.x,
                        Game.RunSystem.Data.Level.PlayerInstance.transform.position.y, -100);    
        }
    }
}

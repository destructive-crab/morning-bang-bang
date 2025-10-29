using banging_code.common;
using banging_code.level;
using banging_code.level.structure;
using banging_code.runs_system;
using MohDIed.Tilemaps;
using MothDIed;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MinimapLogic : MonoBehaviour
{
    public Material notURPMaterial;
    public Tile floor;
    public Tile wall;
    
    private void Start()
    {
        CreateMinimapLevelCopy();
    }

    private void CreateMinimapLevelCopy()
    {
        var minimapGrid = GameObject.Instantiate(Game.G<RunSystem>().Data.Level.Hierarchy.LevelGrid);
        minimapGrid.name = "MINIMAP_LEVEL_COPY";

        foreach (Transform gridChild in minimapGrid.transform)
        {
            gridChild.gameObject.layer = LayerMask.NameToLayer("Minimap");
        }

        var floor = minimapGrid.transform.Find(G_O_NAMES.GLOBAL_FLOOR_TM).GetComponent<Tilemap>();
        floor.ForEachTile<FloorTile>((Vector3Int pos, FloorTile tile) => floor.SetTile(pos, this.floor));

        var walls = minimapGrid.transform.Find(G_O_NAMES.GLOBAL_OBSTACLES_TM).GetComponent<Tilemap>();
        walls.ForEachTile<WallTile>((Vector3Int pos, WallTile tile) => walls.SetTile(pos, this.wall));
        
        foreach (var tilemap in minimapGrid.GetComponentsInChildren<TilemapRenderer>())
        {
            tilemap.material = notURPMaterial;
        }

        Destroy(minimapGrid.GetComponentInChildren<TilemapCollider2D>());
    }

    private void Update()
    {
        if (Game.G<RunSystem>().IsInRun)
        {
            transform.position = new Vector3(Game.G<RunSystem>().Data.Level.PlayerInstance.transform.position.x,
                        Game.G<RunSystem>().Data.Level.PlayerInstance.transform.position.y, -100);    
        }
    }
}

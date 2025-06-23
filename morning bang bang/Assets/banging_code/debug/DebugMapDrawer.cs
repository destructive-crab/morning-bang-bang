using System;
using destructive_code.Tilemaps;
using MothDIed;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.debug
{
    public class DebugMapDrawer
    {
        ~DebugMapDrawer()
        {
            GameObject.Destroy(mapTilemap.gameObject);
        }

        private DebugMapDrawerTiles tiles;
        private Tilemap mapTilemap;
        private Grid debugGrid;

        public void Setup(DebuggerConfig config)
        {
            tiles = config.DebugMapDrawerTiles;
            debugGrid = new GameObject("[DEBUG GRID]").AddComponent<Grid>();
            mapTilemap = new GameObject("[DEBUG MAP TILEMAP]", typeof(TilemapRenderer)).GetComponent<Tilemap>();
            mapTilemap.transform.parent = debugGrid.transform;
            Game.MakeGameObjectPersistent(debugGrid.gameObject);
            mapTilemap.GetComponent<TilemapRenderer>().sortingOrder = 10;
            mapTilemap.gameObject.SetActive(false);
        }

        public void DrawMap()
        {
            if (!Game.RunSystem.IsInRun || Game.RunSystem.Data.Level == null) return;
            
            mapTilemap.gameObject.SetActive(true);
            var allCells = Game.RunSystem.Data.Level.Map.All();
            
            foreach (var cellData in allCells)
            {
                var cellPosition = mapTilemap.WorldToCell(new Vector3(cellData.Position.x, cellData.Position.y));
                if(cellData.CMNIsWalkable)
                {
                    mapTilemap.SetTile(cellPosition, tiles.walkableTile);
                }
                else
                {
                    mapTilemap.SetTile(cellPosition, tiles.obstacleTile);
                }
            }
        }

        public void UpdateMap()
        {
            mapTilemap.ClearAllTiles();
            DrawMap();
        }

        public void HideMap()
        {
            mapTilemap.ClearAllTiles();
            mapTilemap.gameObject.SetActive(false);
        }

        [Serializable]
        public class DebugMapDrawerTiles
        {
            public TileBase walkableTile;
            public TileBase obstacleTile;
        }
    }
}
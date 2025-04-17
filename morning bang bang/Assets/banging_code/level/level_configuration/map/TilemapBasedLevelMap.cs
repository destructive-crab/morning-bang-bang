using banging_code.ai.pathfinding;
using banging_code.common;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.level.level_configuration.map
{
    public sealed class TilemapBasedLevelMap : LevelMap
    {
        private Grid globalGrid;
        private Tilemap obstaclesMap;
        private Tilemap floorMap;

        public TilemapBasedLevelMap(Grid globalGrid)
        {
            this.globalGrid = globalGrid;

            obstaclesMap = globalGrid.transform.Find(G_O_NAMES.GLOBAL_OBSTACLES_TM).GetComponent<Tilemap>();
            floorMap = globalGrid.transform.Find(G_O_NAMES.GLOBAL_FLOOR_TM).GetComponent<Tilemap>();

            foreach (Vector2Int position in obstaclesMap.cellBounds.allPositionsWithin)
            {
                var pos = obstaclesMap.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
                this.UpdateCell(new Vector2Int((int)pos.x, (int)pos.y));
            }
        }

        public override void UpdateCell(Vector2Int position)
        {
            CellData cell = null;
            
            if(map.ContainsKey(position)) cell = map[position];

            if (cell == null)
            {
                cell = new CellData(position);
                map.Add(position, cell);
                cell.Center = obstaclesMap.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
            }
            
            var tile = obstaclesMap.GetTile(new Vector3Int(position.x, position.y, 0)); 
            
            cell.Obstacle = (IObstacleTile)tile;

            tile = floorMap.GetTile(new Vector3Int(position.x, position.y, 0)); 
            
            cell.Floor = tile as IFloorTile;
        }

        public void Tick()
        {
            foreach (var dynamicObstacle in dynamicObstacles)
            {
                Vector2Int cellPos = new Vector2Int((int)dynamicObstacle.transform.position.x,
                    (int)dynamicObstacle.transform.position.y);

                if (map.ContainsKey(cellPos)) map[cellPos].Other = dynamicObstacle;
            }
        }
    }
}
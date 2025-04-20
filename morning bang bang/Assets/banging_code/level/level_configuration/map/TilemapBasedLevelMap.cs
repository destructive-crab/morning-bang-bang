using banging_code.ai.pathfinding;
using MothDIed.Scenes;
using UnityEngine;

namespace banging_code.level.structure.map
{
    public sealed class TilemapBasedLevelMap : LevelMap
    {
        private readonly LevelHierarchyModule hierarchy;

        public TilemapBasedLevelMap(LevelHierarchyModule hierarchy)
        {
            this.hierarchy = hierarchy;
        }

        public override void UpdateCell(Vector2Int position)
        {
            CellData cell = null;
            
            if(map.ContainsKey(position)) cell = map[position];

            if (cell == null)
            {
                cell = new CellData(position);
                map.Add(position, cell);
                cell.Center = hierarchy.ObstaclesTilemap.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
            }
            
            var tile = hierarchy.ObstaclesTilemap.GetTile(new Vector3Int(position.x, position.y, 0)); 
            
            cell.Obstacle = (IObstacleTile)tile;

            tile = hierarchy.FloorTilemap.GetTile(new Vector3Int(position.x, position.y, 0)); 
            
            cell.Floor = tile as IFloorTile;
        }

        public override void Refresh()
        {
            foreach (Vector2Int position in hierarchy.ObstaclesTilemap.cellBounds.allPositionsWithin)
            {
                var pos = hierarchy.ObstaclesTilemap.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
                this.UpdateCell(new Vector2Int((int)pos.x, (int)pos.y));
            }
        }

        public override void UpdateModule(Scene scene)
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
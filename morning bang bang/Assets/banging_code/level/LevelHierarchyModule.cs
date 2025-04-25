using banging_code.common;
using banging_code.common.rooms;
using MothDIed.Scenes.SceneModules;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.level
{
    public class LevelHierarchyModule : SceneModule
    {
        public Transform LevelContainer { get; private set; }
        public Transform RoomsContainer { get; private set; }
        public Grid LevelGrid { get; private set; }
        public Tilemap FloorTilemap { get; private set; }
        public Tilemap ObstaclesTilemap { get; private set; }

        public Room[] Rooms { get; set; }

        public void ClearGeneratedLevel()
        {
            GameObject.DestroyImmediate(LevelContainer.gameObject);
            SetupGeneratedLevelBase();
        }
        public void SetupGeneratedLevelBase()
        {
            //CREATING CONTAINERS
            LevelContainer = new GameObject(G_O_NAMES.LEVEL_CONTAINER).transform;
            RoomsContainer = new GameObject(G_O_NAMES.ROOMS_CONTAINER).transform;
            RoomsContainer.parent = LevelContainer;

            //CREATING GLOBAL MAPS
            LevelGrid = new GameObject(G_O_NAMES.GLOBAL_GRID).AddComponent<Grid>();
            LevelGrid.transform.parent = LevelContainer;

            //floor global map 
            FloorTilemap = new GameObject(G_O_NAMES.GLOBAL_FLOOR_TM, GlobalConfig.GlobalFloorTilemapComponents)
                .GetComponent<Tilemap>();
            FloorTilemap.transform.parent = LevelGrid.transform;

            var globalFloorTilemapRenderer = FloorTilemap.GetComponent<TilemapRenderer>();
            globalFloorTilemapRenderer.sortingLayerName = "Default";
            globalFloorTilemapRenderer.sortOrder = TilemapRenderer.SortOrder.TopLeft;
            
            //obstacles global map
            ObstaclesTilemap = new GameObject(G_O_NAMES.GLOBAL_OBSTACLES_TM, GlobalConfig.GlobalObstaclesTilemapComponents)
                .GetComponent<Tilemap>();
            ObstaclesTilemap.transform.parent = LevelGrid.transform;
           
            var globalObstaclesTilemapRenderer = ObstaclesTilemap.GetComponent<TilemapRenderer>();
            globalObstaclesTilemapRenderer.sortingLayerName = "Default";
            globalObstaclesTilemapRenderer.sortingOrder = 3;
            globalObstaclesTilemapRenderer.sortOrder = TilemapRenderer.SortOrder.TopLeft;
            globalObstaclesTilemapRenderer.mode = TilemapRenderer.Mode.Individual;
        }
    }
}
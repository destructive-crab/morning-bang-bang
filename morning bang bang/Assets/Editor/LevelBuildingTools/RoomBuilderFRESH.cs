using System.Collections.Generic;
using banging_code.common.rooms;
using banging_code.editor.room_builder_tool;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Editor.LevelBuildingTools
{
    public class RoomBuilderFRESH : OdinEditorWindow
    {
        private readonly RoomBuilder builder = new();
        
        [MenuItem("Tools/RoomBuilderFRESH")]
        private static void OpenWindow()
        {
            GetWindow<RoomBuilderFRESH>().Show();
        }

        public List<Room> toEdit = new();
        
         [Button]
         public void UpdateSockets()
         {
             foreach (var room in toEdit)
             {
                 builder.StartEditing(room);
                 builder.CreateRoomSockets();
             }
         }       
         
        [Button]
        public void UpdateCollider()
        {
              foreach (var room in toEdit)
              {
                  builder.StartEditing(room);
                  builder.CreateRoomShapeCollider();
              }           
        }
        
        [Button]
        public void UpdateAll()
        {
              foreach (var room in toEdit)
              {
                  builder.StartEditing(room);
                  
                  builder.CreateRoomSockets();
                  builder.CreateRoomShapeCollider();
              }           
        }
        
        [Button]
        public void AddCurrent()
        {
            if (Selection.gameObjects != null)
            {
                foreach (var go in Selection.gameObjects)
                {
                    if(go.TryGetComponent(out Room room))
                        toEdit.Add(room);
                }
            }
        }
    }
}
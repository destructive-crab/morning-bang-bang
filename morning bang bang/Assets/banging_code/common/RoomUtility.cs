using banging_code.common.rooms;
using UnityEngine;

namespace banging_code.common
{
    public static class RoomUtility
    {
          public static T GetFromContentRoot<T>(this Room room, string path)
              where T : Component
          {
              var target = room.transform.Find(G_O_NAMES.ROOM_CONTENT_ROOT).Find(path);
    
              if (target == null) return null;
              
              return target.GetComponent<T>();
          }
    
          public static T GetFromRoom<T>(this Room room, string path)
              where T : Component
          {
              var target = room.transform.Find(path);
    
              if (target == null)
                  return null;
              
              return target.GetComponent<T>();
      }
    }
}
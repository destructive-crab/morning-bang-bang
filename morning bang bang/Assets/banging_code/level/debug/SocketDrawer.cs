using banging_code.common;
using banging_code.level.rooms;
using UnityEngine;

namespace banging_code.editor.room_builder_tool
{
    public static class SocketDrawer
    {
        public static bool Enabled;
        
        public static void DrawSocketsFor(Room room)
        {
            //drawing sockets
            if (room.SocketsHandler.GetSocketsCount() == 0 || !Enabled)
                return;
            
            foreach (var socket in room.SocketsHandler.GetSockets())
            {
                string iconName = "Arrow_";
                
                switch (socket.Direction)
                {
                    case GameDirection.Left:
                        if (socket.IsForBud) iconName += "L_Y";
                        else                 iconName += "L_B";
                        break;
                    
                    case GameDirection.Right:
                        if (socket.IsForBud) iconName += "R_Y";
                        else                 iconName += "R_B";
                        break;
                    
                    case GameDirection.Top:
                        if (socket.IsForBud) iconName += "T_Y";
                        else                 iconName += "T_B";
                        break;
                    
                    case GameDirection.Bottom:
                        if (socket.IsForBud) iconName += "B_Y";
                        else                 iconName += "B_B";
                        break;
                }

                Gizmos.DrawIcon(room.transform.position + new Vector3(socket.Offset.x, socket.Offset.y), iconName);
            }
        }
    }
}
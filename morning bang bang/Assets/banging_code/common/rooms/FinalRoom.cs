using banging_code.level.light;
using MothDIed.DI;

namespace banging_code.common.rooms
{
    public class FinalRoom : Room
    {
        [Inject]
        private void EnableLight(LightManager lightManager) => lightManager.TurnOn(RoomID);
    }
}
using banging_code.camera_logic;
using banging_code.level.light;
using banging_code.player_logic;
using MothDIed.DI;

namespace banging_code.common.rooms
{
    public class BuisnessRoom : Room
    {
        [Inject] private CCamera cCamera;

        [Inject]
        void InjectLightManager(LightManager lightManager) => lightManager.TurnOn(RoomID);
        
        private PlayerTrigger trigger;
        private BuisnessCameraTarget target;
        
        public override void ProcessContentInRoom()
        {
            base.ProcessContentInRoom();
            trigger = GetComponentInChildren<PlayerTrigger>();
            target = GetComponentInChildren<BuisnessCameraTarget>();
            
            trigger.OnEnter += PlayerEntered;
            trigger.OnExit += PlayerExit;
        }

        private void PlayerEntered(PlayerRoot playerRoot)
        {
            cCamera.EnterBuisnessCamera(target.transform, playerRoot.transform);
        }        
        private void PlayerExit(PlayerRoot playerRoot)
        {
            cCamera.EnterChillCamera();
        }
    }
}
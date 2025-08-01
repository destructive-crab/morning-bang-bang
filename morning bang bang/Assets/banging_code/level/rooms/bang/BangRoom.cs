using System.Collections.Generic;
using System.Linq;
using banging_code.ai;
using banging_code.camera_logic;
using banging_code.health;
using banging_code.level.light;
using MothDIed.DI;

namespace banging_code.common.rooms
{
    public class BangRoom : Room
    {
        public List<Moody> Moodys;
        public List<IBodyDeathRequire> toDie;
        private List<Enemy> entities = new();

        private LightManager lightManager;
        [Inject] private CCamera cCamera;
        [Inject] private EntityPathfindingSystem entityPathfinding;

        [Inject]
        private void InjectLightManager(LightManager lightManager)
        {
            this.lightManager = lightManager;
            lightManager.TurnOn(RoomID);
        }
        
        //callbacks
        private IOnBreakIntoRoom[] onBreakIntoRoomSubs;
        
        public void BreakIntoRoom(BreakArg arg)
        {
            for (var i = 0; i < onBreakIntoRoomSubs.Length; i++)
            {
                onBreakIntoRoomSubs[i].OnBreak();
            }
            
            //???
            var entities = GetComponentsInChildren<Enemy>();
            this.entities = new List<Enemy>(entities);

            entityPathfinding.AddActiveEntities(entities);

            foreach (var entity in entities)
            {
                entity.WakeUp();
                entity.Health.OnDie += OnEntityDie;
            }
            
            lightManager.TurnOn(RoomID);
            cCamera.EnterBangCamera(RoomShapeCollider);
            
        }

        private void OnEntityDie(Enemy enemy)
        {
            if (entities.Contains(enemy))
            {
                entities.Remove(enemy);
            }

            if (entities.Count == 0)
            {
                cCamera.EnterChillCamera();
            }
        }

        public override void ProcessContentInRoom()
        {
            base.ProcessContentInRoom();
            
            foreach (IConnectedWithRoom<BangRoom> obj in ContentRoot.GetComponentsInChildren<IConnectedWithRoom<BangRoom>>())
            {
                obj.InRoom = this;
            }
            
            onBreakIntoRoomSubs = ContentRoot.GetComponentsInChildren<IOnBreakIntoRoom>();
            toDie = new(ContentRoot.GetComponentsInChildren<IBodyDeathRequire>());

            foreach (var body in toDie)
            {
                body.OnDie += OnDie;
            }
            
            Moodys = new List<Moody>(GetComponentsInChildren<Moody>());
        }

        private void OnDie(IBodyDeathRequire require, HitableBody body)
        {
            toDie.Remove(require);
            if (toDie.Count == 0)
            {
                cCamera.EnterChillCamera();
            }
        }
    }
}
using banging_code.level;
using content.commuter_basement_I;
using MothDIed;
using UnityEngine;

namespace banging_code.runs_system
{
    public class RunSystem
    {
        public bool IsInRun => Data != null;
        public RunData Data { get; private set; }

        public void StartDevRun()
        {
            Data = new RunData(Resources.Load<ItemsPoolConfig>("Dev/DevItemsPool").items);
            EnterNewLevel(new CommuterBasementI());
        }

        public void EnterNewLevel(LevelScene level)
        {
            if(Data == null) return;
            
            Game.SwitchTo(level);
        }

        public void SaveRun()
        {
            //saving??
        }

        public void WhileOnLevel(LevelScene levelScene)
        {
            if (Data != null)
            {
                Data.Inventory.WhileOnLevel(levelScene);
            }
        }
        
        public void Die()
        {
            Game.RunSystem.Data.Level.PlayerInstance.gameObject.SetActive(false);
            //Spawn deadbody

            if (Input.anyKey)
            {
                Data = null;
                //delete save???
                
                Game.SwitchTo(new CommonScene("Menu"));
            }
        }
    }

}
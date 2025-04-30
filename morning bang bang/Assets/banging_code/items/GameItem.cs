using UnityEngine;
using UnityEngine.Serialization;

namespace banging_code.items
{
    public abstract class GameItem : ScriptableObject
    { 
        public abstract string ID { get; }

        public int Cost;
        
        public Sprite ItemIcon;
        public Sprite InstanceSprite;
    }
}
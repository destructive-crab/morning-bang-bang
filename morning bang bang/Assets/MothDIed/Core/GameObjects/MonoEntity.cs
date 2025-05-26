using MothDIed.MonoSystems;
using MothDIed.ServiceLocators;
using UnityEngine;

namespace MothDIed
{
    public abstract class MonoEntity : MonoBehaviour
    {
        public abstract string ID { get; }
        
        public readonly ServiceLocator<Component> CachedComponents = new();
        public readonly ServiceLocator<MonoData> Data = new();
        public readonly SystemsContainer Systems = new();

        public override string ToString()
        {
            return $"(name) {gameObject.name}; (id) {ID}";
        }
    }
}
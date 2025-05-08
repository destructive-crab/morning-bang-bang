using MothDIed.MonoSystems;
using MothDIed.ServiceLocators;
using UnityEngine;

namespace MothDIed
{
    public abstract class MonoEntity : MonoBehaviour
    {
        public readonly ServiceLocator<Component> CachedComponents = new();
        public readonly ServiceLocator<MonoData> Data = new();
        public readonly SystemsContainer Systems = new();
    }
}
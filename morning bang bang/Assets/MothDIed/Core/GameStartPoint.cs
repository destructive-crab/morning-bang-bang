using System;
using MothDIed;
using MothDIed.DI;
using UnityEngine;

public abstract class GameStartPoint : MonoBehaviour
{
    public virtual IDependenciesProvider[] GetProviders() { return Array.Empty<IDependenciesProvider>(); }

    private void Start()
    {
        Game.DIKernel.RegisterDependenciesToCore(this);

        StartGame();
    }

    protected abstract void StartGame();
}
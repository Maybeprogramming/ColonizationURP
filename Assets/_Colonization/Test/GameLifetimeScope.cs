using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] HelloScreen _helloScreen;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<HelloVContainer>(Lifetime.Singleton);
        builder.RegisterEntryPoint<GamePresenter>();
        builder.RegisterComponent(_helloScreen);
    }
}

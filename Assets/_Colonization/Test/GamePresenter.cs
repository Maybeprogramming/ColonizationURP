using UnityEngine;
using VContainer.Unity;

public class GamePresenter: IStartable, ITickable
{
    private readonly HelloVContainer _helloService;
    private readonly HelloScreen _helloScreen;

    public GamePresenter(HelloVContainer helloService, HelloScreen helloScreen)
    {
        _helloService = helloService;
        _helloScreen = helloScreen;
    }

    public void Start()
    {
        _helloScreen.HelloButton.onClick.AddListener(() => _helloService.SayHello());
    }

    public void Tick()
    {
        _helloService.SayHello();
    }
}

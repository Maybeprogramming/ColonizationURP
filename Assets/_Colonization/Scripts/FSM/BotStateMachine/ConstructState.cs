using UnityEngine;

public class ConstructState : IState
{
    private const float ConstructDuration = 3f;

    private BotStateMachine _stateMachine;
    private float _timer;
    private BaseFactory _baseFactory;
    private bool _constructionStarted;

    public ConstructState(BotStateMachine botStateMachine)
    {
        _stateMachine = botStateMachine;
    }

    public void Enter()
    {
        _timer = ConstructDuration;
        _constructionStarted = false;
        _baseFactory = Object.FindFirstObjectByType<BaseFactory>();

        Bot bot = _stateMachine.Bot as Bot;

        if (bot != null)
        {
            ToolsProvider tools = bot.GetComponent<ToolsProvider>();
            tools?.Enable();
        }
    }

    public void Exit()
    {
        _stateMachine.Bot.HasConstructTask = false;

        Bot bot = _stateMachine.Bot as Bot;

        if (bot != null)
        {
            ToolsProvider tools = bot.GetComponent<ToolsProvider>();
            tools?.Disable();
        }
    }

    public void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer > 0)
            return;

        if (_constructionStarted)
            return;

        _constructionStarted = true;

        IBot botData = _stateMachine.Bot;
        Bot bot = botData as Bot;

        if (bot == null || _baseFactory == null)
        {
            _stateMachine.TransitionTo<IdleState>();
            return;
        }

        Base oldBase = botData.OwnerBase;
        Vector3 buildPosition = botData.ConstructTargetPosition;

        Base newBase = _baseFactory.Spawn(buildPosition, oldBase);

        oldBase.ClearExpansionFlag();

        bot.SwitchBase(newBase);

        _stateMachine.TransitionTo<IdleState>();
    }
}

using UnityEngine;

public class ConstructState : IState
{
    private const float ConstructDuration = 3f;

    private BotStateMachine _stateMachine;
    private float _timer;
    private bool _constructionStarted;

    public ConstructState(BotStateMachine botStateMachine)
    {
        _stateMachine = botStateMachine;
    }

    public void Enter()
    {
        _timer = ConstructDuration;
        _constructionStarted = false;

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
        BaseFactory baseFactory = _stateMachine.BaseFactory;

        if (bot == null || baseFactory == null)
        {
            _stateMachine.TransitionTo<IdleState>();
            return;
        }

        IBase oldBase = botData.OwnerBase;
        Vector3 buildPosition = botData.ConstructTargetPosition;

        Base newBase = baseFactory.Spawn(buildPosition);

        if (newBase != null)
        {
            oldBase.ClearExpansionFlag();
            bot.SwitchBase(newBase);
        }
        else
        {
            Base baseForLog = oldBase as Base;
            string baseName = baseForLog != null ? baseForLog.name : "unknown";
            Debug.LogWarning($"{nameof(ConstructState)}: Spawn returned null. base='{baseName}', pos={buildPosition}", _stateMachine as MonoBehaviour);
        }

        _stateMachine.TransitionTo<IdleState>();
    }
}


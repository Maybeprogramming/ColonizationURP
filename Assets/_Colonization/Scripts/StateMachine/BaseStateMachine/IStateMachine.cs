public interface IStateMachine
{
    IState GetCurrentState { get; }
    void TransitionTo<T>() where T : IState;
}
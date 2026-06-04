public interface IState
{
    void Enter();
    void Update();
    void Exit();
    bool IsBusy => true;
}

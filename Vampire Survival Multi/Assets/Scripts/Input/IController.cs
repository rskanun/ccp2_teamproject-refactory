public interface IController
{
    public string ID { get; }

    public void OnConnected(MainInput inputActions);
    public void OnDisconnected(MainInput inputActions);
}
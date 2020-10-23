namespace Mud.Server
{
    public interface IClientConsumer
    {
        bool ProcessEvent(GameClient client, ClientOperation operationEvent);
        void ProcessMessage(GameClient client, MudMessage message);
    }
}
namespace er.library.mq.Domain.Bus
{
    using er.library.mq.Domain.Commands;
    using er.library.mq.Domain.Events;

    public interface IEventBus
    {
        Task SendCommand<T>(T command, string actionType) where T : Command;

        Task Publish<T>(T @event, string actionType) where T : Event;
    }
}

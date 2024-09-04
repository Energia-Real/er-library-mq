namespace er.library.mq.Domain.Bus
{
    using er.library.mq.Domain.Commands;
    using er.library.mq.Domain.Events;

    public interface IEventBus
    {
        Task SendCommand<T>(T command) where T : Command;

        void Publish<T>(T @event) where T : Event;
    }
}

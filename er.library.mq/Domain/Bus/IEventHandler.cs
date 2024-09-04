namespace er.library.mq.Domain.Bus
{
    using er.library.mq.Domain.Events;

    public interface IEventHandler<in TEvent> : IEventHandler
    where TEvent : Event
    {
        Task Handle(TEvent @event);
    }

    public interface IEventHandler { }
}

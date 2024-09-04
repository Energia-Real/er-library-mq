namespace er.library.mq.Domain.Commands
{
    using er.library.mq.Domain.Events;

    public abstract class Command : Message
    {
        public DateTime Timestamp { get; protected set; }

        protected Command()
        {
            Timestamp = DateTime.Now;
        }
    }
}

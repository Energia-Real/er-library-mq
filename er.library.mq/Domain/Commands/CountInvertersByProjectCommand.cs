namespace er.library.mq.Domain.Commands
{
    public class CountInvertersByProyectCommand : Command
    {
        public CountInvertersByProyectCommand(string proyectExternalId, int count)
        {
            ProyectExternalId = proyectExternalId;
            Count = count;
        }

        public string ProyectExternalId { get; }
        public int Count { get; }
    }
}

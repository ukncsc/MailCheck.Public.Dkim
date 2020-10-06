namespace MailCheck.Dkim.Entity.Entity.Notifiers
{
    public interface IChangeNotifier
    {
        void Handle(DkimEntityState state, Common.Messaging.Abstractions.Message message);
    }
}
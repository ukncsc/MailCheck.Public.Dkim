namespace MailCheck.Dkim.Scheduler.Dao.Model
{
    public class DkimSchedulerState
    {
        public DkimSchedulerState(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
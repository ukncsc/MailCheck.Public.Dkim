namespace MailCheck.Dkim.Evaluator.Domain
{
    public class ServiceType : Tag
    {
        public ServiceType(string value, ServiceTypeType type,  bool isImplicit = false) 
            : base(value, isImplicit)
        {
            Type = type;
        }

        public ServiceTypeType Type { get; }
    }
}

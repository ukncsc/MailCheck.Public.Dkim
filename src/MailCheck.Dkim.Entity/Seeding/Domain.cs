using System;

namespace MailCheck.Dkim.Entity.Seeding
{
    internal class Domain
    {
        public Domain(string name, string createdBy, DateTime createdDate)
        {
            Name = name;
            CreatedBy = createdBy;
            CreatedDate = createdDate;
        }

        public string Name { get; }
        public string CreatedBy { get; }
        public DateTime CreatedDate { get; }
    }
}
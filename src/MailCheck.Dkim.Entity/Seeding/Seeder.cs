using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;
using MailCheck.Dkim.Contracts.External;

namespace MailCheck.Dkim.Entity.Seeding
{
    internal interface ISeeder
    {
        Task SeedDomainCreated();
    }

    internal class Seeder : ISeeder
    {
        private readonly IDomainDao _domainDao;
        private readonly ISqsPublisher _publisher;
        private readonly ISeederConfig _config;

        public Seeder(IDomainDao domainDao, ISqsPublisher publisher, ISeederConfig config)
        {
            _domainDao = domainDao;
            _publisher = publisher;
            _config = config;
        }

        public async Task SeedDomainCreated()
        {
            List<Domain> domains = await _domainDao.GetDomains();

            List<DomainCreated> domainCreateds =
                domains.Select(_ => new DomainCreated(_.Name, _.CreatedBy, _.CreatedDate)).ToList();

            int count = 0;
            foreach (IEnumerable<DomainCreated> domainCreated in domainCreateds.Batch(10))
            {
                List<Message> messages = domainCreated.Cast<Message>().ToList();
                await _publisher.Publish(messages, _config.SnsTopicToSeedArn);
                Console.WriteLine($"Processed {count += messages.Count} events.");
            }
        }
    }
}

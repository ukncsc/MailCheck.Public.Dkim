using System;
using System.Collections.Generic;
using MailCheck.Dkim.Api.Domain;
using MailCheck.Dkim.Api.Mapping;
using NUnit.Framework;

namespace MailCheck.Dkim.Api.Test.Mapping
{
    [TestFixture]
    public class EntityToApiMapperTests
    {
        private EntityToApiMapper _entityToApiMapper;

        [SetUp]
        public void SetUp()
        {
            _entityToApiMapper = new EntityToApiMapper();
        }

        [Test]
        public void ItShouldMapCorrectly()
        {
            string domain = "domain";
            DateTime lastUpdated = new DateTime(2019, 01, 01);
            EntityDkimState entityState = EntityDkimState.Evaluated;

            string selector = "selector";
            string cname = "cname";

            string record = "record";
            string markDown = "markDown";
            string entity = "entity";
            string messageType = "messageType";
            List<EntityDkimRecord> entityDkimRecords = new List<EntityDkimRecord>
                {new EntityDkimRecord(new EntityDnsRecord(record, new List<string>()), new List<EntityMessage>{new EntityMessage(entity, messageType, markDown) })};

            List<EntityDkimSelector> entityDkimSelectors = new List<EntityDkimSelector>
                {new EntityDkimSelector(selector, entityDkimRecords, "cname", null)};

            EntityDkimEntityState state = new EntityDkimEntityState(domain, entityState, 4, new DateTime(), lastUpdated, new DateTime(), entityDkimSelectors);

            DkimResponse result = _entityToApiMapper.ToDkimResponse(state);

            Assert.AreEqual(domain, result.Domain);
            Assert.AreEqual(lastUpdated, result.LastUpdated);

            Assert.AreEqual(State.Evaluated, result.State);

            Assert.AreEqual(1, result.Selectors.Count);
            Assert.AreEqual(cname, result.Selectors[0].CName);
            Assert.AreEqual(selector, result.Selectors[0].Selector);

            Assert.AreEqual(1, result.Selectors[0].Records.Count);
            Assert.AreEqual(record, result.Selectors[0].Records[0].Record);

            Assert.AreEqual(1, result.Selectors[0].Messages.Count);
            Assert.AreEqual(messageType, result.Selectors[0].Messages[0].Severity);
            Assert.AreEqual(markDown, result.Selectors[0].Messages[0].MarkDown);
            Assert.AreEqual(entity, result.Selectors[0].Messages[0].Message);
        }
    }
}
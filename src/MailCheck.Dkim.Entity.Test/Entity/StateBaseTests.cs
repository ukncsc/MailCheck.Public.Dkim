using System;
using MailCheck.Dkim.Entity.Entity;
using NUnit.Framework;

namespace MailCheck.Dkim.Entity.Test.Entity
{
    [TestFixture]
    public class StateBaseTests
    {
        private const string Source1 = "source_1";
        private const string Source2 = "source_2";

        [Test]
        public void CanUpdateTrueIfNoEvents()
        {
            State state = new State();
            Assert.True(state.CanUpdate(Source1, DateTime.UtcNow));
        }

        [Test]
        public void CanUpdateTrueIfNoEventForSource()
        {
            State state = new State();
            state.UpdateSource(Source2, DateTime.UtcNow);
            Assert.True(state.CanUpdate(Source1, DateTime.UtcNow));
        }

        [Test]
        public void CanUpdateTrueIfEventIsNewerThanPrevious()
        {
            State state = new State();

            DateTime dateTime = DateTime.UtcNow;
            
            state.UpdateSource(Source1, dateTime);
            Assert.True(state.CanUpdate(Source1, dateTime.AddSeconds(1)));
        }

        [Test]
        public void CanUpdateFalseIfEventOlderThanPrevious()
        {
            State state = new State();

            DateTime dateTime = DateTime.UtcNow;

            state.UpdateSource(Source1, dateTime.AddSeconds(-1));
            Assert.True(state.CanUpdate(Source1, dateTime));
        }

        private class State : StateBase{}
    }
}

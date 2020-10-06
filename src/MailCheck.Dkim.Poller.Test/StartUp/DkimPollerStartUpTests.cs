using MailCheck.Dkim.Poller.StartUp;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace MailCheck.Dkim.Poller.Test.StartUp
{
    [TestFixture]
    public class DkimPollerStartUpTests
    {
        [Test]
        public void ItShouldNotFail()
        {
            DkimPollerStartUp sut = new DkimPollerStartUp();

            IServiceCollection services = new ServiceCollection();

            sut.ConfigureServices(services);

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            Assert.That(serviceProvider, Is.Not.Null);
        }
    }
}

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dqe.Domain.Messaging;
using Dqe.Domain.Model;
using Dqe.Infrastructure.EntityIoC;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories.Custom;

namespace Dqe.Infrastructure.Tests
{
    [TestClass]
// ReSharper disable InconsistentNaming
    public class When_Using_The_Persistence_Infrastructure
// ReSharper restore InconsistentNaming
    {
        [TestInitialize]
        public void SetUp()
        {
            Initializer.Initialize(true);
            EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
        }

        private static IMessenger MockUpMessenger()
        {
            return new Mock<IMessenger>().Object;
        }

        private static object[] EntityDependencyResolverOnResolveConstructorArguments(object sender, ResolveConstructorArgumentsArgs args)
        {
            if (args.EntityType.IsAssignableFrom(typeof(UserAccount))) return new object[] { MockUpMessenger() };
            return null;
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void SampleEntity_Is_Persistable()
        // ReSharper restore InconsistentNaming
        {
            var entity = new UserAccount(MockUpMessenger())
            {
                Email = "admin@admin.com",
                AccountPassword = "admin",
                AccountRole = "Admin",
                FirstName = "Admin",
                LastName = "Admin"
            };
            UnitOfWorkProvider.CommandRepository.Add(entity);
            UnitOfWorkProvider.TransactionManager.Commit();
            entity = new UserAccountRepository().Get(entity.Id);
            Assert.IsNotNull(entity);
            Assert.AreEqual(1, entity.Id);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Dynamic_Transformer_Projects_Dynamic_Object()
        // ReSharper restore InconsistentNaming
        {
            var entity = new UserAccount(MockUpMessenger())
            {
                Email = "admin@admin.com",
                AccountPassword = "admin",
                AccountRole = "Admin",
                FirstName = "Admin",
                LastName = "Admin"
            };
            UnitOfWorkProvider.CommandRepository.Add(entity);
            UnitOfWorkProvider.TransactionManager.Commit();
            var userEmails = new UserAccountRepository().GetUserEmailsProjection(entity.Id).ToList();
            Assert.AreEqual(userEmails.Count, 1);
            Assert.AreEqual(userEmails[0].FirstName, "Admin");
            Assert.AreEqual(userEmails[0].LastName, "Admin");
            Assert.AreEqual(userEmails[0].Email, "admin@admin.com");
        }
    }
}

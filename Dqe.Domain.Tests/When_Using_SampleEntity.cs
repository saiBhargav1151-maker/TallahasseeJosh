using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dqe.Domain.Messaging;
using Dqe.Domain.Model;

namespace Dqe.Domain.Tests
{
    [TestClass]
// ReSharper disable InconsistentNaming
    public class When_Using_UserAccount
// ReSharper restore InconsistentNaming
    {
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void The_Correct_Notify_Is_Invoked_On_Validation()
// ReSharper restore InconsistentNaming
        {
            var messenger = new Mock<IMessenger>();
            var entity = new UserAccount(messenger.Object);
            Assert.AreEqual(0, entity.Id);
        }
    }
}

using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.ApplicationServices.Tests
{
    [TestClass]
// ReSharper disable InconsistentNaming
    public class When_Using_CustomPrincipal
// ReSharper restore InconsistentNaming
    {
        private IIdentity MockUpIdentity()
        {
            return new Mock<IIdentity>().Object;
        }

        private IContextService MockUpContextService()
        {
            return new Mock<IContextService>().Object;
        }

        private IUserAccountRepository MockUserAccountRepository()
        {
            return new Mock<IUserAccountRepository>().Object;
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void IsInRole_Returns_False_For_Any_Role()
// ReSharper restore InconsistentNaming
        {
            var principal = new CustomPrincipal(MockUpIdentity(), MockUpContextService(), MockUserAccountRepository());
            Assert.IsFalse(principal.IsInRole("any"));
        }
    }
}

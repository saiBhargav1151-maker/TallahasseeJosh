using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dqe.Web.Controllers;

namespace Dqe.Web.Tests
{
    [TestClass]
// ReSharper disable InconsistentNaming
    public class When_Using_Home_Controller
// ReSharper restore InconsistentNaming
    {
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void Home_Controller_Index_Returns_View()
// ReSharper restore InconsistentNaming
        {
            var c = new HomeController();
            var v = c.Index();
            Assert.IsInstanceOfType(v, typeof(ViewResult));
        }
    }
}

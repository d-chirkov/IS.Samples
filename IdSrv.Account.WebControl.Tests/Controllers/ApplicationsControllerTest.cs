namespace IdSrv.Account.WebControl.Controllers.Tests
{
    using System;
    using NUnit.Framework;
    using Moq;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;
    using System.Collections.Generic;
    using IdSrv.Account.WebControl.Models;
    using System.Web.Mvc;
    using System.Threading.Tasks;

    [TestFixture]
    class ApplicationsControllerTest
    {
        public Mock<IApplicationService> ApplicationServiceMock { get; set; }
        [SetUp] 
        public void SetUp()
        {
            ApplicationServiceMock = new Mock<IApplicationService>();
        }

        [Test]
        public void Ctor_DoesNotThrow_When_PassingAnyApplicationService()
        {
            Assert.DoesNotThrow(() => new ApplicationsController(ApplicationServiceMock.Object));
        }

        [Test]
        public void Ctor_ThrowNullReferenceException_When_PassingNullInsteadUserService()
        {
            Assert.Throws<NullReferenceException>(() => new ApplicationsController(null));
        }
    }
}

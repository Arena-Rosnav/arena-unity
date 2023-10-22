#if RESTSERVER_VISUALSCRIPTING
using System;
using NUnit.Framework;
using RestServer.VisualScripting;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestVisualHttpHeaderExtension {
        [Test]
        public void Test01() {
            foreach (var e in Enum.GetValues(typeof(VisualHttpHeader))) {
                var vhh = (VisualHttpHeader)e;
                Assert.NotNull(vhh.ConvertToString());
            }
            
            Assert.AreEqual("allow", VisualHttpHeader.Allow.ConvertToString());
            Assert.AreEqual("Set-Cookie", VisualHttpHeader.SetCookie.ConvertToString());
        }
    }
}
#endif
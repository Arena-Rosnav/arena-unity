using NUnit.Framework;
using RestServer.Helper;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestSimpleInterlock {
        public class TestPathHelper {
            [Test]
            public void Test01() {
                var si = new SimpleInterlock();

                var w0 = si.DoWork();

                Assert.AreEqual(true, si.isRunning);

                w0.Dispose();

                Assert.AreEqual(false, si.isRunning);
            }
        }
    }
}
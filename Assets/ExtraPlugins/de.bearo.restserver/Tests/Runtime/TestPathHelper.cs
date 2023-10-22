using NUnit.Framework;
using RestServer.Helper;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestPathHelper {
        [Test]
        public void TestIncludeStartingSlash() {
            Assert.AreEqual("/", PathHelper.EnsureSlashPrefix(""));
            Assert.AreEqual("/", PathHelper.EnsureSlashPrefix("/"));
            Assert.AreEqual("/aaa", PathHelper.EnsureSlashPrefix("/aaa"));
            Assert.AreEqual("/bbb", PathHelper.EnsureSlashPrefix("bbb"));
        }
        
        [Test]
        public void TestRemoveEndingSlash() {
            Assert.AreEqual("", PathHelper.RemoveEndingSlash( ""));
            Assert.AreEqual("", PathHelper.RemoveEndingSlash("/"));
            Assert.AreEqual("aaa", PathHelper.RemoveEndingSlash("aaa/"));
            Assert.AreEqual("bbb", PathHelper.RemoveEndingSlash("bbb"));
        }
        
        [Test]
        public void TestConcatPath() {
            Assert.AreEqual("a/b", PathHelper.ConcatPath("a", "b"));
            Assert.AreEqual("a/b", PathHelper.ConcatPath("a/", "b"));
            Assert.AreEqual("a/b", PathHelper.ConcatPath("a", "/b"));
            Assert.AreEqual("a/b", PathHelper.ConcatPath("a/", "/b"));
            Assert.AreEqual("a", PathHelper.ConcatPath("a", ""));
            Assert.AreEqual("/b", PathHelper.ConcatPath("", "b"));
        }
    }
}
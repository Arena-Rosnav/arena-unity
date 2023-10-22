using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RestServer.Helper;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestHeaderBuilder {
        [Test]
        public void Test_Add_01() {
            var h = new HeaderBuilder();
            h.withHeader("a", "b");

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual("b", d["a"][0]);
        }

        [Test]
        public void Test_Add_02() {
            var h = new HeaderBuilder("a", "b");

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual("b", d["a"][0]);
        }

        [Test]
        public void Test_Add_03() {
            var h = new HeaderBuilder();
            h.withHeader("a", "b1");
            h.withHeader("a", "b2");

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual(2, d["a"].Capacity);
            Assert.AreEqual("b1", d["a"][0]);
            Assert.AreEqual("b2", d["a"][1]);
        }

        [Test]
        public void Test_Add_04() {
            var h = new HeaderBuilder();
            h.withHeader("a", new[] {"b1", "b2"});

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual(2, d["a"].Capacity);
            Assert.AreEqual("b1", d["a"][0]);
            Assert.AreEqual("b2", d["a"][1]);
        }

        [Test]
        public void Test_Add_05() {
            var h = new HeaderBuilder();
            h.withHeader("a", new[] {"b1", "b2"});
            h.withHeader("a", new[] {"b1", "b2"});

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual(4, d["a"].Capacity);
            Assert.AreEqual("b1", d["a"][0]);
            Assert.AreEqual("b2", d["a"][1]);
            Assert.AreEqual("b1", d["a"][2]);
            Assert.AreEqual("b2", d["a"][3]);
        }

        [Test]
        public void Test_SetIfNotExists_01() {
            var h = new HeaderBuilder();
            h.withHeader("a", new[] {"b1", "b2"});

            h.withSetIfNotExists("b", "set");

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.Contains("b", d.Keys);
            Assert.AreEqual(2, d["a"].Count);
            Assert.AreEqual(1, d["b"].Count);
            Assert.AreEqual("b1", d["a"][0]);
            Assert.AreEqual("b2", d["a"][1]);
            Assert.AreEqual("set", d["b"][0]);
        }

        [Test]
        public void Test_SetIfNotExists_02() {
            var h = new HeaderBuilder();
            h.withHeader("a", new[] {"b1", "b2"});

            h.withSetIfNotExists("a", "set");

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual(2, d["a"].Count);
            Assert.AreEqual("b1", d["a"][0]);
            Assert.AreEqual("b2", d["a"][1]);
        }

        [Test]
        public void Test_SetIfNotExists_03() {
            var h = new HeaderBuilder();
            h.withHeader("a", new[] {"b1", "b2"});

            h.withSetIfNotExists("a", new[] {"b3", "b4"});

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual(2, d["a"].Count);
            Assert.AreEqual("b1", d["a"][0]);
            Assert.AreEqual("b2", d["a"][1]);
        }

        [Test]
        public void Test_SetIfNotExists_04() {
            var h = new HeaderBuilder();
            h.withSetIfNotExists("a", new[] {"b3", "b4"});

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual(2, d["a"].Count);
            Assert.AreEqual("b3", d["a"][0]);
            Assert.AreEqual("b4", d["a"][1]);
        }

        [Test]
        public void Test_Overwrite_01() {
            var h = new HeaderBuilder();
            h.withHeader("a", new[] {"b1", "b2"});

            h.withOverwriteHeader("a", "set");

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual(1, d["a"].Count);
            Assert.AreEqual("set", d["a"][0]);
        }

        [Test]
        public void Test_Overwrite_02() {
            var h = new HeaderBuilder();
            h.withHeader("a", new[] {"b1", "b2"});

            h.withOverwriteHeader("b", "set");

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.Contains("b", d.Keys);
            Assert.AreEqual(2, d["a"].Count);
            Assert.AreEqual(1, d["b"].Count);
            Assert.AreEqual("b1", d["a"][0]);
            Assert.AreEqual("b2", d["a"][1]);
            Assert.AreEqual("set", d["b"][0]);
        }

        [Test]
        public void Test_Overwrite_03() {
            var h = new HeaderBuilder();
            h.withHeader("a", new[] {"b1", "b2"});

            h.withOverwriteHeader("a", new[] {"b3", "b4"});

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual(2, d["a"].Count);
            Assert.AreEqual("b3", d["a"][0]);
            Assert.AreEqual("b4", d["a"][1]);
        }

        [Test]
        public void Test_RemoveHeaderName_01() {
            var h = new HeaderBuilder();
            h.withHeader("a", new[] {"b1", "b2"});
            h.withHeader("b", "-");

            h.withRemoveHeaderName("a");

            var d = (Dictionary<string, List<string>>) h;
            Assert.True(!d.Keys.Contains("a"));
            Assert.Contains("b", d.Keys);
        }

        [Test]
        public void Test_RemoveHeader_02() {
            var h = new HeaderBuilder();
            h.withHeader("a", new[] {"b1", "b2", "b1"});

            h.withRemoveHeader("a", "b1");

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual(1, d["a"].Count);
            Assert.AreEqual("b2", d["a"][0]);
        }

        [Test]
        public void Test_Ctor_01() {
            var temp = new Dictionary<string, List<string>>();
            temp.Add("a", new List<string>(new[] {"b1", "b2"}));

            var h = new HeaderBuilder(temp);

            var d = (Dictionary<string, List<string>>) h;
            Assert.Contains("a", d.Keys);
            Assert.AreEqual(2, d["a"].Count);
            Assert.AreEqual("b1", d["a"][0]);
            Assert.AreEqual("b2", d["a"][1]);
        }

        [Test]
        public void Test_DeepClone_01() {
            var orig = new Dictionary<string, List<string>>();
            orig.Add("a", new List<string>(new[] {"b1", "b2"}));
            orig.Add("b", new List<string>(new[] {"c1"}));

            var clone = HeaderBuilder.DeepClone(orig);
            clone.Remove("b");

            Assert.Contains("a", orig.Keys);
            Assert.Contains("b", orig.Keys);

            Assert.Contains("a", clone.Keys);
            Assert.False(clone.Keys.Contains("b"));
            Assert.AreEqual("b1", clone["a"][0]);
            Assert.AreEqual("b2", clone["a"][1]);
        }
    }
}
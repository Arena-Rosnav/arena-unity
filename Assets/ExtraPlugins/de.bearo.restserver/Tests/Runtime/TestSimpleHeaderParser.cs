using NUnit.Framework;
using RestServer.MultipartFormData;

namespace de.bearo.restserver.Tests.Runtime {

    public class TestSimpleContentDispositionParser {
        [Test]
        public void Test01() {
            const string header = "multipart/form-data; charset=utf-8; boundary=__X_PAW_BOUNDARY__";
            
            var p = SimpleHeaderValueParser.Parse(header);
            
            Assert.AreEqual("utf-8", p.GetPart("charset"));
            Assert.AreEqual("__X_PAW_BOUNDARY__", p.GetPart("boundary"));
            Assert.IsTrue(p.HasPart("multipart/form-data"));
        }
        
        [Test]
        public void Test02() {
            const string header = "form-data; name=\"files[]\"; filename=\"photo1.jpg\"";
            
            var p = SimpleHeaderValueParser.Parse(header);
            
            Assert.AreEqual("files[]", p.GetPart("name"));
            Assert.AreEqual("photo1.jpg", p.GetPart("filename"));
            Assert.IsTrue(p.HasPart("form-data"));
        }
    }
}
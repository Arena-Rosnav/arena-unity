using System;
using NUnit.Framework;
using RestServer.Helper;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestPathParamHelper {
        [Test]
        public void TestParsing01() {
            const string endpoint = "/path/{param1}/path2/{param2}";
            var pathParams = PathParamHelper.ParseEndpoint(endpoint, out var regex);
            
            Assert.AreEqual(2, pathParams.Count);
            Assert.AreEqual("param1", pathParams["param1"].Name);
            Assert.AreEqual("param2", pathParams["param2"].Name);
            Assert.AreEqual(1, pathParams["param1"].Index);
            Assert.AreEqual(3, pathParams["param2"].Index);
            Assert.AreEqual("^/path/[^/]+/path2/[^/]+$", regex.ToString());
        }
        
        [Test]
        public void TestParsing02() {
            const string endpoint = "/path/{param1:type1}/path2/{param2:type2}";
            var pathParams = PathParamHelper.ParseEndpoint(endpoint, out var regex);
            
            Assert.AreEqual(2, pathParams.Count);
            Assert.AreEqual("param1", pathParams["param1"].Name);
            Assert.AreEqual("param2", pathParams["param2"].Name);
            Assert.AreEqual(1, pathParams["param1"].Index);
            Assert.AreEqual(3, pathParams["param2"].Index);
            Assert.AreEqual("^/path/[^/]+/path2/[^/]+$", regex.ToString());
            
            Assert.AreEqual("type1", pathParams["param1"].Type);
            Assert.AreEqual("type2", pathParams["param2"].Type);
        }
        
        [Test]
        public void TestParsingErrors() {
            Assert.Throws<ArgumentException>(() => PathParamHelper.ParseEndpoint("/path/{param/type1}/path2/{param2:type2}", out _));
            
            Assert.Throws<ArgumentException>(() => PathParamHelper.ParseEndpoint("/path/{param/path2/{param2:type2}", out _));
            
            Assert.Throws<ArgumentException>(() => PathParamHelper.ParseEndpoint("/path/param}/path2/{param2:type2}", out _));
            
            Assert.Throws<ArgumentException>(() => PathParamHelper.ParseEndpoint("/path/{param}/path2/{param}", out _));
            
            Assert.Throws<ArgumentException>(() => PathParamHelper.ParseEndpoint("/path/notallowed{param}/path2/{param2}", out _));
        }
        
        [Test]
        public void TestRequestParsing01() {
            var uri = new Uri("http://localhost/path/value_param1/path2/value_param2");
            const string endpoint = "/path/{param1:type1}/path2/{param2:type2}";
            var pathParams = PathParamHelper.ParseEndpoint(endpoint, out _);

            var parsedParams = PathParamHelper.ParseRequestUri(uri, pathParams);
            
            Assert.AreEqual("value_param1", parsedParams["param1"].ValueString);
            Assert.AreEqual("value_param2", parsedParams["param2"].ValueString);
        }
        
        [Test]
        public void TestRequestParsing02() {
            var uri = new Uri("http://localhost/path/1234/path2/5678");
            const string endpoint = "/path/{param1:int}/path2/{param2:int}";
            var pathParams = PathParamHelper.ParseEndpoint(endpoint, out _);

            var parsedParams = PathParamHelper.ParseRequestUri(uri, pathParams);
            
            Assert.AreEqual("1234", parsedParams["param1"].ValueString);
            Assert.AreEqual("int", parsedParams["param1"].Type);
            
            Assert.AreEqual("5678", parsedParams["param2"].ValueString);
            Assert.AreEqual("int", parsedParams["param2"].Type);
        }
    }
}
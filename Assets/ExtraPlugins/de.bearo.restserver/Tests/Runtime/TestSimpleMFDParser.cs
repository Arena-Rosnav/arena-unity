using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RestServer;
using RestServer.MultipartFormData;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestSimpleMFDParser {
        private static readonly string TestData01 = TrimAllLines(
            @"--boundary
            Content-Disposition: form-data; name=""file1""
              
            datadatafile1
            --boundary
            Content-Disposition: form-data; name=""file2"";
              
            datadatafile2
            --boundary
            Content-Disposition: form-data; name=""file3""; filename=""data.txt""
            Content-Type: text/plain
            
            datadatafile3 
            --boundary
            Content-Disposition: form-data; Name=""file4""; Filename=""awesome.txt""
            Content-Type: text/plain

            datadatafile4
            --boundary
            Content-Disposition: form-data; name=""file4""

            datadatafile5
            --boundary
            Content-Disposition: form-data; name=""file5""

            datadatafile6
            --boundary--"
        );

        [Test]
        public void Test01() {
            var headers = new Dictionary<string, IList<string>>();
            var contentTypeValue = new List<string> { "multipart/form-data; boundary=boundary" };
            headers.Add(HttpHeader.CONTENT_TYPE, contentTypeValue);

            var mfds = new SimpleMFDParser().Parse(headers, Encoding.UTF8.GetBytes(TestData01));

            {
                var mfd1 = mfds[0];
                Assert.AreEqual("datadatafile1", mfd1.DataAsString());
                Assert.Contains("form-data", (ICollection)mfd1.ContentHeaders["Content-Disposition"].Keys);
                Assert.AreEqual("file1", mfd1.Name);
            }

            {
                var mfd2 = mfds[1];
                Assert.AreEqual("datadatafile2", mfd2.DataAsString());
                Assert.Contains("Content-Disposition", (ICollection)mfd2.ContentHeaders.Keys);
                Assert.Contains("form-data", (ICollection)mfd2.ContentHeaders["Content-Disposition"].Keys);
                Assert.AreEqual("file2", mfd2.Name);
            }

            {
                var mfd3 = mfds[2];
                Assert.AreEqual("datadatafile3", mfd3.DataAsString());
                Assert.Contains("Content-Disposition", (ICollection)mfd3.ContentHeaders.Keys);
                Assert.Contains("form-data", (ICollection)mfd3.ContentHeaders["Content-Disposition"].Keys);
                Assert.AreEqual("file3", mfd3.Name);
                Assert.AreEqual("data.txt", mfd3.Filename);
            }

            {
                var mfd4 = mfds[3];
                Assert.AreEqual("datadatafile4", mfd4.DataAsString());
                Assert.Contains("Content-Disposition", (ICollection)mfd4.ContentHeaders.Keys);
                Assert.Contains("form-data", (ICollection)mfd4.ContentHeaders["Content-Disposition"].Keys);
                Assert.AreEqual("file4", mfd4.Name);
                Assert.AreEqual("awesome.txt", mfd4.Filename);
            }

            {
                var mfd5 = mfds[4];
                Assert.AreEqual("datadatafile5", mfd5.DataAsString());
                Assert.Contains("Content-Disposition", (ICollection)mfd5.ContentHeaders.Keys);
                Assert.Contains("form-data", (ICollection)mfd5.ContentHeaders["Content-Disposition"].Keys);
                Assert.AreEqual("file4", mfd5.Name);
            }

            {
                var mfd6 = mfds[5];
                Assert.AreEqual("datadatafile6", mfd6.DataAsString());
                Assert.Contains("Content-Disposition", (ICollection)mfd6.ContentHeaders.Keys);
                Assert.Contains("form-data", (ICollection)mfd6.ContentHeaders["Content-Disposition"].Keys);
                Assert.AreEqual("file5", mfd6.Name);
            }
        }

        public static string TrimAllLines(string input) {
            return
                string.Concat(
                    input.Split('\n')
                        .Select(x => x.Trim())
                        .Aggregate((first, second) => first + '\n' + second)
                        .Where(x => x != '\r'));
        }
    }
}
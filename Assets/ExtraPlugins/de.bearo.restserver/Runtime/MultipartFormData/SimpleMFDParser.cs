using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RestServer.MultipartFormData {
    /// <summary>
    /// Very simple multipart/form-data parser that works synchronously. It is not intended to provide/parse all cases, but to have a simple parser that
    /// works in most simple cases. If more functionality is needed, there are pretty advanced open source projects already.
    ///
    /// This parser works synchronously and should be executed in the endpoint handler directly to benefit from the background thread. This parser might have
    /// problems with large files.
    /// </summary>
    public class SimpleMFDParser {
        /// <summary>
        /// Parse given request. See <see cref="SimpleMFDParser"/> for details.
        ///
        /// NOTE THAT THIS PARSER ONLY SUPPORTS A SUBSET OF THE multipart/form-data STANDARD. If more functionality is needed, please include established
        /// open source solutions (or spin your own parser :) )
        /// </summary>
        public List<MultiFormDataElement> Parse(RestRequest request) {
            return Parse(request.Headers, request.BodyBytes);
        }


        /// <summary>
        /// Parse given request. See <see cref="SimpleMFDParser"/> for details.
        ///
        /// NOTE THAT THIS PARSER ONLY SUPPORTS A SUBSET OF THE multipart/form-data STANDARD. If more functionality is needed, please include established
        /// open source solutions (or spin your own parser :) )
        /// </summary>
        public List<MultiFormDataElement> Parse(IDictionary<string, IList<string>> headers, byte[] bodyBytes) {
            if (!headers.ContainsKey(HttpHeader.CONTENT_TYPE)) {
                throw new SystemException($"No {HttpHeader.CONTENT_TYPE} found.");
            }

            var contentType = headers[HttpHeader.CONTENT_TYPE];
            var pContentType = SimpleHeaderValueParser.Parse(contentType[0]);

            if (!pContentType.HasPart("multipart/form-data")) {
                throw new SystemException("No multipart/form-data request.");
            }

            var encoding = Encoding.UTF8;
            var boundary = pContentType.GetPart("boundary");
            var boundaryBytes = encoding.GetBytes("--" + boundary);

            var holmes = new BoyerMooreSearch(boundaryBytes);

            var rb = bodyBytes;
            var lastByteIdx = -1;
            var ret = new List<MultiFormDataElement>();
            foreach (var partByteIdx in holmes.Search(rb)) {
                if (lastByteIdx == -1) {
                    // every document starts with a boundary, skip it
                    lastByteIdx = partByteIdx + boundaryBytes.Length;
                    lastByteIdx += NewLineCharacters(bodyBytes, lastByteIdx);
                    continue;
                }

                var partLength = partByteIdx - lastByteIdx;
                var partBytes = new byte[partLength];
                Array.Copy(rb, lastByteIdx, partBytes, 0, partLength);

                ret.Add(ParsePart(partBytes));

                lastByteIdx = partByteIdx + boundaryBytes.Length;
                lastByteIdx += NewLineCharacters(bodyBytes, lastByteIdx);
            }

            return ret;
        }

        private MultiFormDataElement ParsePart(byte[] partBytes) {
            var holmes = new BoyerMooreSearch(new[] { (byte)10 }); // LF

            var lastByteIdx = 0;
            var headers = new Dictionary<string, IDictionary<string, string>>();
            foreach (var segmentByteIdx in holmes.Search(partBytes)) {
                var partLength = segmentByteIdx - lastByteIdx;
                if (partBytes[segmentByteIdx - 1] == 13) { // part ends with CR
                    partLength--;
                }

                if (partLength > 0) {
                    var segmentBytes = new byte[partLength];
                    Array.Copy(partBytes, lastByteIdx, segmentBytes, 0, partLength);

                    var segmentStr = Encoding.UTF8.GetString(segmentBytes);
                    ParsePartHeaderElement(headers, segmentStr);

                    lastByteIdx = segmentByteIdx + 1; // advance after LF
                }
                else {
                    // if part length == 0 we assume that the header stopped and the binary data begins
                    var startDataIdx = segmentByteIdx + 1;
                    var segmentLength = partBytes.Length - startDataIdx - 1;
                    var data = new byte[segmentLength];
                    Array.Copy(partBytes, startDataIdx, data, 0, segmentLength);

                    return new MultiFormDataElement(headers, data);
                }
            }

            return new MultiFormDataElement(headers, null);
        }

        private static int NewLineCharacters(byte[] bodyBytes, int position) {
            return bodyBytes[position] == '\r' ? 2 : 1;
        }

        private static void ParsePartHeaderElement(IDictionary<string, IDictionary<string, string>> headers, string segmentStr) {
            var ts = segmentStr.Trim();
            var parts = ts.Split(':');
            if (parts.Length != 2) { // not in the format key: value - ignore
                return;
            }

            var headerKey = parts[0];
            var headerValues = SimpleHeaderValueParser.Parse(parts[1]);
            headers.Add(headerKey, new ReadOnlyDictionary<string, string>(headerValues.GetParts()));
        }
    }

    /// <summary>
    /// Parsed multipart form-data element of the request (data+headers)
    /// </summary>
    public class MultiFormDataElement {
        /// <summary>
        /// multipart form-data specific header for this element
        /// </summary>
        public readonly IDictionary<string, IDictionary<string, string>> ContentHeaders;

        /// <summary>
        /// Payload of the element in byte[]
        /// </summary>
        public readonly byte[] Data;

        /// <summary>
        /// Content-Disposition Name header; null if it wasn't included in the request
        /// </summary>
        public string Name => GetHeader(HttpHeader.CONTENT_DISPOSITION, "name");

        /// <summary>
        /// Content-Disposition Filename header; null if it wasn't included in the request
        /// </summary>
        public string Filename => GetHeader(HttpHeader.CONTENT_DISPOSITION, "filename");

        public MultiFormDataElement(IDictionary<string, IDictionary<string, string>> contentHeaders, byte[] data) {
            ContentHeaders = contentHeaders;
            Data = data;
        }

        /// <summary>
        /// Convert byte[] data to string with Unicode UTF-8 encoding
        /// </summary>
        public string DataAsString() {
            return DataAsString(Encoding.UTF8);
        }

        /// <summary>
        /// Convert byte[] data to string with specified encoding
        /// </summary>
        public string DataAsString(Encoding encoding) {
            return Data == null ? null : encoding.GetString(Data);
        }

        private string GetHeader(string headerName, string key) {
            if (!ContentHeaders.TryGetValue(headerName, out var values)) {
                return null;
            }

            return values.TryGetValue(key, out var value) ? value : null;
        }
    }
}
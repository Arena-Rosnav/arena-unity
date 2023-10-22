using System.Collections.Generic;
using System.Collections.ObjectModel;
using RestServer.NetCoreServer;

namespace RestServer.Helper {
    public static class RequestHeaderHelper {

        /// <summary>
        /// Convert the list of headers on the http request to an easy accessible, readonly dictionary.
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static IDictionary<string, IList<string>> ToReadOnlyHeaderDict(HttpRequest httpRequest) {
            var tempDict = ToHeaderDict(httpRequest);

            var readOnlyLists = new Dictionary<string, IList<string>>();
            foreach (var kv in tempDict) {
                readOnlyLists.Add(kv.Key, new ReadOnlyCollection<string>(kv.Value));
            }

            return new ReadOnlyDictionary<string, IList<string>>(readOnlyLists);
        }
        
        /// <summary>
        /// Convert the list of headers on the http request to an easy accessible dictionary.
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> ToHeaderDict(HttpRequest httpRequest) {
            var ret = new Dictionary<string, List<string>>();

            for (var i = 0; i < httpRequest.Headers; i++) {
                var h = httpRequest.Header(i);

                if (!ret.ContainsKey(h.Item1)) {
                    ret[h.Item1] = new List<string>();
                }

                ret[h.Item1].Add(h.Item2);
            }
            
            return ret;
        }
    }
}
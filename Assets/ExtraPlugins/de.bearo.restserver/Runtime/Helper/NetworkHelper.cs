using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace RestServer.Helper {
    public class NetworkHelper {
        /// <summary>
        /// Find all possible IPs that the server is listening on when started (only works on started rest servers).
        /// We are enumerating over all IFs and find those that match the parameters of the endpoint. This is not 100% accurate
        /// and other configurations might interfere with this (ex firewalls, virus scanners, routing tables, etc.).
        /// </summary>
        /// <param name="restServer">Reference to a started rest server instance.</param>
        public static List<RestServerIPInfo> GetPossibleListenIPs(RestServer restServer) {
            if (Equals(restServer.ListenAddress, IPAddress.Loopback)) {
                var netIf = NetworkInterface.GetAllNetworkInterfaces()[NetworkInterface.LoopbackInterfaceIndex];
                return new List<RestServerIPInfo>(new[] { new RestServerIPInfo(netIf, IPAddress.Loopback) });
            }
            
            var ret = new List<RestServerIPInfo>();
            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces()) {
                var ipProps = netInterface.GetIPProperties();
                foreach (var address in ipProps.UnicastAddresses) {
                    if (address.Address.AddressFamily != restServer.Server.Endpoint.AddressFamily) {
                        continue;
                    }

                    ret.Add(new RestServerIPInfo(netInterface, address.Address));
                }
            }

            return ret;
        }
    }

    /// <summary>
    /// Records IP information on which the rest server might listen to
    /// </summary>
    public struct RestServerIPInfo {
        /// <summary>
        /// Network interface that is used for the IP
        /// </summary>
        public readonly NetworkInterface NetworkInterface;
        
        /// <summary>
        /// IPAddress of the network interface
        /// </summary>
        public readonly IPAddress IPAddress;

        public RestServerIPInfo(NetworkInterface networkInterface, IPAddress ipAddress) {
            NetworkInterface = networkInterface;
            IPAddress = ipAddress;
        }

        public override string ToString() {
            return $"{NetworkInterface.Name} ({IPAddress})";
        }
    }
}
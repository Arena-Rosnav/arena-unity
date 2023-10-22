#if RESTSERVER_VISUALSCRIPTING

using System;

namespace RestServer.VisualScripting {
    /// <summary>
    /// Used for all XXXHeaderEnum Nodes for the switch.
    /// </summary>
    public enum VisualHttpHeader {
        Allow,
        AccessControlAllowOrigin,
        AccessControlAllowCredentials,
        AccessControlExposeHeaders,
        AccessControlMaxAge,
        AccessControlAllowMethods,
        AccessControlAllowHeaders,
        CacheControl,
        ContentDisposition,
        ContentEncoding,
        ContentLength,
        ContentType,
        ETag,
        LastModified,
        SetCookie,
        WWWAuthenticate
    }

    public static class VisualHttpHeaderExtension {
        public static string ConvertToString(this VisualHttpHeader header) {
            switch (header) {
                case VisualHttpHeader.Allow:
                    return HttpHeader.ALLOW;
                case VisualHttpHeader.AccessControlAllowOrigin:
                    return HttpHeader.ACCESS_CONTROL_ALLOW_ORIGIN;
                case VisualHttpHeader.AccessControlAllowCredentials:
                    return HttpHeader.ACCESS_CONTROL_ALLOW_CREDENTIALS;
                case VisualHttpHeader.AccessControlExposeHeaders:
                    return HttpHeader.ACCESS_CONTROL_EXPOSE_HEADERS;
                case VisualHttpHeader.AccessControlMaxAge:
                    return HttpHeader.ACCESS_CONTROL_MAX_AGE;
                case VisualHttpHeader.AccessControlAllowMethods:
                    return HttpHeader.ACCESS_CONTROL_ALLOW_METHODS;
                case VisualHttpHeader.AccessControlAllowHeaders:
                    return HttpHeader.ACCESS_CONTROL_ALLOW_HEADERS;
                case VisualHttpHeader.CacheControl:
                    return HttpHeader.CACHE_CONTROL;
                case VisualHttpHeader.ContentDisposition:
                    return HttpHeader.CONTENT_DISPOSITION;
                case VisualHttpHeader.ContentEncoding:
                    return HttpHeader.CONTENT_ENCODING;
                case VisualHttpHeader.ContentLength:
                    return HttpHeader.CONTENT_LENGTH;
                case VisualHttpHeader.ContentType:
                    return HttpHeader.CONTENT_TYPE;
                case VisualHttpHeader.ETag:
                    return HttpHeader.E_TAG;
                case VisualHttpHeader.LastModified:
                    return HttpHeader.LAST_MODIFIED;
                case VisualHttpHeader.SetCookie:
                    return HttpHeader.SET_COOKIE;
                case VisualHttpHeader.WWWAuthenticate:
                    return HttpHeader.WWW_AUTHENTICATE;
            }

            throw new ArgumentOutOfRangeException(nameof(header), header, null);
        }
    }
}
#endif
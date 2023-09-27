using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Foundation.WebSocket
{
    public interface IWebSocketOptions
    {
        IEnumerable<(string Key, string Value)> Headers { get; set; }

        IEnumerable<string> SubProtocols { get; set; }

        IWebProxy Proxy { get; set; }

        int SendQueueLimit { get; set; }

        TimeSpan SendCacheItemTimeout { get; set; }

        ushort SendDelay { get; set; }

        ReconnectStrategy ReconnectStrategy { get; set; }

        bool DebugMode { get; set; }

        bool IgnoreCertErrors { get; set; }

        CookieContainer Cookies { get; set; }

        X509CertificateCollection ClientCertificates { get; set; }

        int DisconnectWait { get; set; }
    }
}

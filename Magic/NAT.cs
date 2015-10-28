using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

namespace Hacknet.Magic
{
    public class NAT
    {
        private static TimeSpan _timeout = new TimeSpan(0, 0, 0, 3);
        private static string _descUrl;
        private static string _serviceUrl;
        private static string _eventUrl;

        public static TimeSpan TimeOut
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        public static bool Discover()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            socket.ReceiveTimeout = 5000;
            var s1 =
                "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nST:upnp:rootdevice\r\nMAN:\"ssdp:discover\"\r\nMX:3\r\n\r\n";
            DebugLog.add(s1);
            var bytes = Encoding.ASCII.GetBytes(s1);
            var ipEndPoint = new IPEndPoint(IPAddress.Broadcast, 1900);
            var numArray = new byte[4096];
            var now = DateTime.Now;
            do
            {
                socket.SendTo(bytes, ipEndPoint);
                socket.SendTo(bytes, ipEndPoint);
                socket.SendTo(bytes, ipEndPoint);
                int count;
                do
                {
                    try
                    {
                        count = socket.Receive(numArray);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        count = 0;
                    }
                    var s2 = Encoding.ASCII.GetString(numArray, 0, count).ToLower();
                    DebugLog.add(s2);
                    if (s2.Contains("upnp:rootdevice"))
                    {
                        var str = s2.Substring(s2.ToLower().IndexOf("location:") + 9);
                        var resp = str.Substring(0, str.IndexOf("\r")).Trim();
                        if (!string.IsNullOrEmpty(_serviceUrl = GetServiceUrl(resp)))
                        {
                            _descUrl = resp;
                            return true;
                        }
                    }
                } while (count > 0);
            } while (now.Subtract(DateTime.Now) < _timeout);
            return false;
        }

        private static string GetServiceUrl(string resp)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(WebRequest.Create(resp).GetResponse().GetResponseStream());
                var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                nsmgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
                if (
                    !xmlDocument.SelectSingleNode("//tns:device/tns:deviceType/text()", nsmgr)
                        .Value.Contains("InternetGatewayDevice"))
                    return null;
                var xmlNode1 =
                    xmlDocument.SelectSingleNode(
                        "//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:controlURL/text()",
                        nsmgr);
                if (xmlNode1 == null)
                {
                    var xmlNode2 =
                        xmlDocument.SelectSingleNode(
                            "//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANPPPConnection:1\"]/tns:controlURL/text()",
                            nsmgr);
                    if (xmlNode2 == null)
                        return null;
                    var xmlNode3 =
                        xmlDocument.SelectSingleNode(
                            "//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANPPPConnection:1\"]/tns:eventSubURL/text()",
                            nsmgr);
                    _eventUrl = CombineUrls(resp, xmlNode3.Value);
                    return CombineUrls(resp, xmlNode2.Value);
                }
                var xmlNode4 =
                    xmlDocument.SelectSingleNode(
                        "//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:eventSubURL/text()",
                        nsmgr);
                _eventUrl = CombineUrls(resp, xmlNode4.Value);
                return CombineUrls(resp, xmlNode1.Value);
            }
            catch
            {
                return null;
            }
        }

        private static string CombineUrls(string resp, string p)
        {
            var num = resp.IndexOf("://");
            var length = resp.IndexOf('/', num + 3);
            return resp.Substring(0, length) + p;
        }

        public static void ForwardPort(int port, ProtocolType protocol, string description)
        {
            if (string.IsNullOrEmpty(_serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");
            SOAPRequest(_serviceUrl,
                "<u:AddPortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\"><NewRemoteHost></NewRemoteHost><NewExternalPort>" +
                port + "</NewExternalPort><NewProtocol>" + protocol.ToString().ToUpper() +
                "</NewProtocol><NewInternalPort>" + port + "</NewInternalPort><NewInternalClient>" +
                Dns.GetHostAddresses(Dns.GetHostName())[0] +
                "</NewInternalClient><NewEnabled>1</NewEnabled><NewPortMappingDescription>" + description +
                "</NewPortMappingDescription><NewLeaseDuration>0</NewLeaseDuration></u:AddPortMapping>",
                "AddPortMapping");
        }

        public static void DeleteForwardingRule(int port, ProtocolType protocol)
        {
            if (string.IsNullOrEmpty(_serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");
            SOAPRequest(_serviceUrl,
                "<u:DeletePortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\"><NewRemoteHost></NewRemoteHost><NewExternalPort>" +
                port + "</NewExternalPort><NewProtocol>" + protocol.ToString().ToUpper() +
                "</NewProtocol></u:DeletePortMapping>", "DeletePortMapping");
        }

        public static IPAddress GetExternalIP()
        {
            if (string.IsNullOrEmpty(_serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");
            var xmlDocument = SOAPRequest(_serviceUrl,
                "<u:GetExternalIPAddress xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\"></u:GetExternalIPAddress>",
                "GetExternalIPAddress");
            var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
            nsmgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
            return IPAddress.Parse(xmlDocument.SelectSingleNode("//NewExternalIPAddress/text()", nsmgr).Value);
        }

        private static XmlDocument SOAPRequest(string url, string soap, string function)
        {
            var s =
                "<?xml version=\"1.0\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><s:Body>" +
                soap + "</s:Body></s:Envelope>";
            var webRequest = WebRequest.Create(url);
            webRequest.Method = "POST";
            var bytes = Encoding.UTF8.GetBytes(s);
            webRequest.Headers.Add("SOAPACTION", "\"urn:schemas-upnp-org:service:WANIPConnection:1#" + function + "\"");
            webRequest.ContentType = "text/xml; charset=\"utf-8\"";
            webRequest.ContentLength = bytes.Length;
            webRequest.GetRequestStream().Write(bytes, 0, bytes.Length);
            var xmlDocument = new XmlDocument();
            var responseStream = webRequest.GetResponse().GetResponseStream();
            xmlDocument.Load(responseStream);
            return xmlDocument;
        }
    }
}
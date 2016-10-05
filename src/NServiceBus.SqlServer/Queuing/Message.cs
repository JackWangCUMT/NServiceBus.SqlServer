namespace NServiceBus.Transport.SQLServer
{
    using System.Collections.Generic;

    class Message
    {
        public Message(string transportId, Dictionary<string, string> headers, byte[] body, string forwardDestination)
        {
            TransportId = transportId;
            Body = body;
            ForwardDestination = forwardDestination;
            Headers = headers;
        }

        public string TransportId { get; }
        public byte[] Body { get; }
        public string ForwardDestination { get; }
        public Dictionary<string, string> Headers { get; }
    }
}
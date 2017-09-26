using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.DomainDriven.EventSourcing.EventStore
{
    public class EventStoreConfiguration
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string IpAddress { get; private set; }
        public int Port { get; private set; }
        public int MaxNumberOfStreamEventsToRead { get; private set; } = 4096;

        public EventStoreConfiguration(string username, string password, string ipAddress, int port, int maxNumberOfStreamsToRead)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            Port = port;
            MaxNumberOfStreamEventsToRead = maxNumberOfStreamsToRead;
        }
    }
}

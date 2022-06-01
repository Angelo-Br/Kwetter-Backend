using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQLibrary
{
    public static class RoutingKeyType
    {
        public const string UsernameUpdated = "username.updated";
        public const string UserDeleted = "user.deleted";
    }
}

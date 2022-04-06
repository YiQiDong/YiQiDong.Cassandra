using Cassandra;
using System;
using System.Collections.Generic;
using System.Text;

namespace YiQiDong.Cassandra.Utils
{
    public class DbUtils
    {
        public static void UseSession(string host, int port, string user, string password, Action<ISession> sessionHandler)
        {
            var builder = Cluster.Builder()
           .AddContactPoint(host)
           .WithPort(port);
            if (!string.IsNullOrEmpty(user))
                builder = builder.WithCredentials(user, password);

            using (var cluster = builder.Build())
            using (var session = cluster.Connect())
                sessionHandler.Invoke(session);
        }
    }
}

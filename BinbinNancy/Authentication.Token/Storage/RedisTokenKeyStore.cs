using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Redis;

namespace Nancy.Authentication.Token.Storage {
    public class RedisTokenKeyStore : ITokenKeyStore {
        public class RedisConfiguration {

            public RedisConfiguration(string connection) {
                ParseRedisConnecting(connection);
            }
            public string host;
            public int port;
            public string password;

            public void ParseRedisConnecting(string redisConStr) {

                string[] redisConsAt = redisConStr.Split('@');
                string[] redisConsColon;
                password = string.Empty;

                if (redisConsAt.Length > 1) {
                    password = redisConsAt[0];
                    redisConsColon = redisConsAt[1].Split(':');
                } else {
                    redisConsColon = redisConStr.Split(':');
                }

                host = redisConsColon[0];
                port = Convert.ToInt32(redisConsColon[1]);
                //more info: http://goo.gl/srA2uT
            }

            public RedisClient GetClient() {
                var redisClient = new RedisClient(host, port);
                if (!string.IsNullOrEmpty(password))
                    redisClient.Password = password;
                return redisClient;
            }
        }
        private readonly IRedisClient _client;
        private string _keyStoreName = "keyStore";

        public RedisTokenKeyStore()
            : this(new RedisClient()) {

        }

        public RedisTokenKeyStore(RedisConfiguration configuration)
            : this(configuration.GetClient()) {

        }
        public RedisTokenKeyStore(IRedisClient client) {
            _client = client;
        }
        public IDictionary<DateTime, byte[]> Retrieve() {
            return this._client.Get<IDictionary<DateTime, byte[]>>(_keyStoreName);
        }

        public void Store(IDictionary<DateTime, byte[]> keys) {
            this._client.Set(_keyStoreName, keys);
        }

        public void Purge() {
            this._client.ExpireEntryIn(_keyStoreName, TimeSpan.Zero);
        }

        public RedisTokenKeyStore WithKeyStoreName(string name) {
            this._keyStoreName = name;
            return this;
        }


    }
}

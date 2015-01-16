using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelerikFriendLocator.Wrappers
{
    public class Message
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "s_facebook_id")]
        public string Sender { get; set; }

        [JsonProperty(PropertyName = "r_facebook_id")]
        public string Receiver { get; set; }
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
    }
}

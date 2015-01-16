using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TelerikFriendLocator.Wrappers
{
    public class ResponseWrapper
    {
        [JsonProperty(PropertyName = "message")] 
        public List<User> FriendsInRange { get; set; }
    }
}

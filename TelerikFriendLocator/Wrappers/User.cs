using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelerikFriendLocator.Wrappers
{
    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "lastname")]
        public string Lastname { get; set; }
        [JsonProperty(PropertyName = "firstname")]
        public string Firstname { get; set; }

        [JsonProperty(PropertyName = "facebookid")]
        public string FacebookId { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public decimal Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public decimal Longitude { get; set; }

        [JsonProperty(PropertyName = "range")]
        public decimal Range { get; set; }

        [JsonProperty(PropertyName = "canbeseen")]
        public bool Visible { get; set; }

        [JsonProperty(PropertyName = "pictureurl")]
        public string PictureUrl { get; set; }

    }
}

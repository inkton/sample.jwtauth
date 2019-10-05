using System.Collections.Generic;
using Newtonsoft.Json;
using Inkton.Nest.Cloud;

namespace Jwtauth.Model
{
    [Cloudname("industry")]
    public class Industry : CloudObject
    {
        private int _id;
        private string _tag;
        private string _name;

        [JsonProperty("id")]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        [JsonProperty("tag")]
        public string Tag
        {
            get => _tag;
            set => SetProperty(ref _tag, value);
        }

        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public override string ToString()
        {
            return string.Format(
                "{0} - {1}", _tag, _name);
        }

        public override string CloudKey
        {
            get { return _id.ToString(); }
        }

        public List<Share> Shares { get; set; }        
    }
}
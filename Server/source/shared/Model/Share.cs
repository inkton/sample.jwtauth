using Newtonsoft.Json;
using Inkton.Nest.Cloud;

namespace Jwtauth.Model
{
    [Cloudname("share")]
    public class Share : CloudObject
    {
        private int _id;
        private int _industryId;
        private string _tag;
        private decimal _price;

        [JsonProperty("id")]
        public int Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }
        
        [JsonProperty("industry_id")]
        public int IndustryId
        {
            get => _industryId;
            set => SetProperty(ref _industryId, value);
        }

        public string Tag
        {
            get { return _tag; }
            set { SetProperty(ref _tag, value); }
        }

        public decimal Price
        {
            get { return _price; }
            set { SetProperty(ref _price, value); }
        }

        public override string CloudKey
        {
            get { return _id.ToString(); }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} - {1}", _tag, _price);
        }
    }
}

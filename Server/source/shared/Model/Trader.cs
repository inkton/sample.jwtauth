using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Inkton.Nest.Cloud;
using Inkton.Nest.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jwtauth.Model
{
    [Cloudname("trader")]
    public class Trader : User
    {
        private DateTime _dateJoined;
        
        [JsonProperty("date_joined")]
        public DateTime DateJoined
        {
            get => _dateJoined;
            set => SetProperty(ref _dateJoined, value);
        }

        [NotMapped]
        [JsonIgnore]
        public override string CloudKey
        {
            get { return Id == 0 ? Email : Id.ToString(); }
        }

        public override string ToString()
        {
            return $"{FirstName} {LastName}, Joined {DateJoined}";
        }
    }
}

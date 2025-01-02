using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request.ThirdParty.GHN {
    public class ShopRequest {
        public int district_id { get; set; }
        public string ward_code { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
    }
}

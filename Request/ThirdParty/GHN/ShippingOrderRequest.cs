using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request.ThirdParty.GHN {
    public class ShippingOrderItem {
        public string name { get; set; }
        public string code { get; set; }
        public int quantity { get; set; }
        public int weight { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class ShippingOrderRequest {
        public int payment_type_id { get; set; } = 1;
        public string note { get; set; }
        public string required_note { get; set; } = "KHONGCHOXEMHANG";
        public string return_name { get; set; }
        public string return_phone { get; set; }
        public string return_address { get; set; }
        public string return_ward_code { get; set; }
        public int return_district_id { get; set; }
        public string to_name { get; set; }
        public string to_phone { get; set; }
        public string to_address { get; set; }
        public string to_ward_code { get; set; }
        public int to_district_id { get; set; }
        public int weight { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int service_id { get; set; } = 0;
        public int service_type_id { get; set; } = 2;
        public object coupon { get; set; }
        public List<ShippingOrderItem> items { get; set; }
        public int insurance_value { get; set; } = 0;
    }
}

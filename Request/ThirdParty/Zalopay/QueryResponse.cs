using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Request.ThirdParty.Zalopay {
    public class QueryResponse {
        public int return_code { get; set; }
        public string return_message { get; set; }
        public int sub_return_code { get; set; }
        public string sub_return_message { get; set;}
        public bool is_processing { get; set; }
        public decimal amount { get; set; }
        public BigInteger zp_trans_id { get; set; }
        public BigInteger server_time { get; set; }
        public decimal discount_amount { get; set; }
    }
}

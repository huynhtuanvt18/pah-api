using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request.ThirdParty.Zalopay {
    public class TopupRequest {
        public decimal Topup { get; set; }
        public int AppId { get; set; }
        public string AppTransId { get; set; }
        public string Mac { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request.ThirdParty.Zalopay {
    public class QueryRequest {
        public int app_id {  get; set; }
        public string app_trans_id { get; set; }
        public string mac { get; set; }

        public void SetData() {
            app_id = 2553;
            app_trans_id = "231010_1696918495971";
            mac = "8b9275afa32899761269b514de85e693603789f03a1a0a414200ef5c7ec2348b";
        }

        public void SetDataSuccess1() {
            app_id = 2553;
            app_trans_id = "231010_1696931155812";
            mac = "1d877da5e2f7b2fe85a907a7d47a6b71cc0c82a7435f49be70a87ddea0f47c11";
        }

        public void SetDataSuccess2() {
            app_id = 2553;
            app_trans_id = "231010_1696931218467";
            mac = "6762335e95c53fc8ac219067cd6a76371899f75a1e03841470ac0586df3eab00";
        }

        public void SetDataInProcess1() {
            app_id = 2553;
            app_trans_id = "231010_1696931263859";
            mac = "965fce8b22559816404e55b7384b6580836cd94196007b2bd95b32a610f9dab7";
        }

        public void SetDataInProcess2() {
            app_id = 2553;
            app_trans_id = "231010_1696931327485";
            mac = "b7de6610e71a3570bd6987940ed69d85be81c40a78daad84ad2cf791c12b76ae";
        }
    }
}

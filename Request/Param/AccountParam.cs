using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request.Param {
    public class AccountParam {
        [Range(0, 1)]
        public int? Status { get; set; }
        [Range(0, 5)]
        public int? Role { get; set; }
        public string Name { get; set; }
        [Range(0, 1)]
        public int? Gender { get; set; }
    }
}

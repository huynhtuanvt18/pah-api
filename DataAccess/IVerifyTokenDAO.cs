using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public interface IVerifyTokenDAO {
        public void Create(VerifyToken verifyToken);
        public void Update(VerifyToken verifyToken);
        public VerifyToken Get(int id);
    }
}

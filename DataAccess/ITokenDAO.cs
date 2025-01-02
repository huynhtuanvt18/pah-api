using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public interface ITokenDAO {
        public void UpdateToken(Token token);
        public void Add(Token token);
        public Token Get(int id);
        public Token GetSavedRefreshToken(int id, string refreshToken);

        public Token GetResetToken(int id, string token, DateTime date);
    }
}

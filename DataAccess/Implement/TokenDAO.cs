using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement {
    public class TokenDAO : DataAccessBase<Token>, ITokenDAO {
        public TokenDAO(PlatformAntiquesHandicraftsContext context) : base(context) {
        }

        public void Add(Token token) {
            Create(token);
        }

        public Token Get(int id) {
            return GetAll().FirstOrDefault(p => p.Id == id);
        }

        public Token GetResetToken(int id, string token, DateTime date) {
            return GetAll().FirstOrDefault(p => p.Id == id && token.Equals(p.RefreshToken) && p.ExpiryTime >= date);
        }

        public Token GetSavedRefreshToken(int id, string refreshToken) {
            return GetAll().FirstOrDefault(p => p.Id == id && p.RefreshToken.Equals(refreshToken));
        }

        public void UpdateToken(Token token) {
            Update(token);
        }
    }
}

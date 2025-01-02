using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Service {
    public interface ITokenService {
        public string GenerateAccessToken(int id);
        public string GenerateRefreshToken();
        public string GenerateResetToken();
        public string GenerateVerifyToken(int length);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}

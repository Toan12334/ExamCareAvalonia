using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamCare.Services
{
    public class TokenService
    {
        private string _accessToken;

        public void SetToken(string token)
        {
            _accessToken = token;
        }

        public string GetToken()
        {
            Debug.WriteLine("accresstoken is:" + _accessToken);
            return _accessToken;
        }

        public void ClearToken()
        {
            _accessToken = null;
        }
    }
}

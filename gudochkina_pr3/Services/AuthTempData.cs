using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gudochkina_pr3.Services
{
    public static class AuthTempData
    {
        private static Dictionary<string, TempAuthData> _tempData = new Dictionary<string, TempAuthData>();

        public static void SaveVerificationCode(string email, string code, int userId)
        {
            var data = new TempAuthData
            {
                Email = email,
                Code = code,
                UserId = userId,
                ExpirationTime = DateTime.Now.AddMinutes(10),
                CreatedAt = DateTime.Now
            };

            RemoveExpiredCodes();

            _tempData[email] = data;
        }

        public static bool VerifyCode(string email, string code, out int? userId)
        {
            userId = null;

            RemoveExpiredCodes();

            if (_tempData.ContainsKey(email))
            {
                var data = _tempData[email];
                if (data.Code == code && DateTime.Now <= data.ExpirationTime)
                {
                    userId = data.UserId;
                    return true;
                }
            }

            return false;
        }

        public static void RemoveCode(string email)
        {
            if (_tempData.ContainsKey(email))
            {
                _tempData.Remove(email);
            }
        }

        private static void RemoveExpiredCodes()
        {
            var expiredKeys = new List<string>();
            foreach (var item in _tempData)
            {
                if (DateTime.Now > item.Value.ExpirationTime)
                {
                    expiredKeys.Add(item.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                _tempData.Remove(key);
            }
        }

        private class TempAuthData
        {
            public string Email { get; set; }
            public string Code { get; set; }
            public int UserId { get; set; }
            public DateTime ExpirationTime { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}

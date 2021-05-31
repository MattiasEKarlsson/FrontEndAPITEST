using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

#nullable disable

namespace FrontEndAPITEST.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public byte[] UserHash { get; set; }
        public byte[] UserSalt { get; set; }
        public string UserRole { get; set; }


        public void CreatePassword(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                UserSalt = hmac.Key;
                UserHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public bool ValidatePassword(string password)
        {
            using (var hmac = new HMACSHA512(UserSalt))
            {
                var _uhach = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < _uhach.Length; i++)
                {
                    if (_uhach[i] != UserHash[i])
                        return false;
                }
            }

            return true;
        }
    }
}

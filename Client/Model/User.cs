using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Model
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public Token Token { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }
}

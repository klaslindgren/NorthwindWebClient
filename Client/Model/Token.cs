using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Model
{
    public class Token
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
    }
}

using System;

namespace Client.Model
{
    public class Response
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public Token Token { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }
}
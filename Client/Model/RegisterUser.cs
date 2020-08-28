using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Model
{
    public class RegisterUser
    {

        public string Title { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public bool AcceptTerms { get; set; }
    }
}

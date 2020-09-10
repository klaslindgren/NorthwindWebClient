using System;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using System.Text.Unicode;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Client.Model;
using System.Net;
using System.Runtime.CompilerServices;
using System.Net.Http.Headers;

namespace Client
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var client = new HttpClient();
            var regUser = new RegisterUser
            {
                //Title = "Mr",
                //FirstName = "Klas",
                //LastName = "Kalas",
                //Email = "k@k.se",
                //Password = "Potatis1.",
                //ConfirmPassword = "Potatis1.",
                //AcceptTerms = true

                Title = "A",
                FirstName = "Ase",
                LastName = "Deo",
                Email = "A@d.se",
                Password = "Sallad1.",
                ConfirmPassword = "Sallad1.",
                AcceptTerms = true
            };

            var authUser = new User { Email = "k@k.se", Password = "Potatis1." };   //  Logga in

            //await RegisterUser(client, regUser);      // Skapa användare

            var user = await AuthenticateUser(client, authUser);        //  Autentisera
            await RefreshToken(client, user);       // Refresha Token
            await ShowAccounts(client, user);       // Kolla access
            await RefreshToken(client, user);       // Refresha Token
            await ShowAccounts(client, user);       // Kolla access
            Console.ReadLine();
        }

        private static async Task<AccountResponse> RegisterUser(HttpClient client, RegisterUser user)
        {
            var register = await client.PostAsJsonAsync("http://localhost:4001/accounts/register", user);
            var newUser = new AccountResponse();
            if (register.IsSuccessStatusCode)
            {
                var response = await register.Content.ReadAsAsync<AccountResponse>();
                newUser = response;
            };
            return newUser;
        }

        private static async Task<AuthenticateResponse> AuthenticateUser(HttpClient client, User user)
        {
            var authenticated = await client.PostAsJsonAsync("http://localhost:4001/accounts/authenticate", user);
            var legitUser = new AuthenticateResponse();

            if (authenticated.IsSuccessStatusCode)
            {
                var response = await authenticated.Content.ReadAsAsync<AuthenticateResponse>();

                legitUser = response;

                Console.WriteLine(legitUser.JwtToken);
            }

            return legitUser;
        }

        private static async Task<AuthenticateResponse> RefreshToken(HttpClient client, AuthenticateResponse user) 
        {

            // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.RefreshToken);
            RefreshTokenRequest token = new RefreshTokenRequest() { RefreshToken = user.RefreshToken };
            var response = await client.PostAsJsonAsync("http://localhost:4001/accounts/refresh-token", token);

            if (response.IsSuccessStatusCode)
            {
                var updateToken = await response.Content.ReadAsAsync<AuthenticateResponse>();
                user.RefreshToken = updateToken.RefreshToken;
                user.JwtToken = updateToken.JwtToken;
            }
        
            return user;
        }

        private static async Task<string> ShowAccounts(HttpClient client, AuthenticateResponse user) 
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.JwtToken);
            var response = await client.GetAsync("http://localhost:4001/accounts/");

            if (response.IsSuccessStatusCode)
            {
                var print = await response.Content.ReadAsStringAsync();
                Console.WriteLine(print);
            }
            
            else
                Console.WriteLine("Access denied");

            return "";
        }
    }

}

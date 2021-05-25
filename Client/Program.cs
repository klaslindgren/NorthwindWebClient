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
using Client.Data;

namespace Client
{
    class Program
    {
        static ClientDBContext context = new ClientDBContext();
        static HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {

            var regUser = new RegisterUser
            {
                //UserName = "Klaskalas",
                //FirstName = "Klas",
                //LastName = "Kalas",
                //Password = "Potatis1.",
                //ConfirmPassword = "Potatis1.",
                //Country = "Sweden"

                UserName = "A",
                FirstName = "Ase",
                LastName = "Deo",
                Password = "Sallad1.",
                ConfirmPassword = "Sallad1.",
                Country = "Sweden"
            };

            //var authUser = new User { Email = "k@k.se", Password = "Potatis1." };   //  Logga in
            Console.ReadLine();
            await RegisterUser(regUser);      // Skapa användare

            //var user = await AuthenticateUser(client, authUser);        //  Autentisera
            //await RefreshToken(client, user);       // Refresha Token
            //await ShowAccounts(client, user);       // Kolla access
            //await RefreshToken(client, user);       // Refresha Token
            //await ShowAccounts(client, user);       // Kolla access
            Console.ReadLine();
        }

        private static async Task RegisterUser(RegisterUser request)
        {
            try
            {
                var register = await client.PostAsJsonAsync("http://localhost:5001/api/user/register", request);
                var response = await register.Content.ReadAsAsync<Response>();

                if (register.IsSuccessStatusCode)
                {
                    User newUser = new User()
                    {
                        UserName = request.UserName
                    };
                    context.Add(newUser);
                    await context.SaveChangesAsync();
                };

                Console.WriteLine(response.Message);
            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
            
        }

        private static async Task AuthenticateUser(LoginModel request)
        {
            var authenticate = await client.PostAsJsonAsync("http://localhost:5001/api/user/login", request);
            var response = await authenticate.Content.ReadAsAsync<Response>();

            if (authenticate.IsSuccessStatusCode)
            {
                User 
                context.Add(newUser);
                await context.SaveChangesAsync();
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

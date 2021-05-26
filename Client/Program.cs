using Client.Data;
using Client.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static ClientDBContext context = new ClientDBContext();
        static HttpClient client = new HttpClient();

        // ACTIVE USER, Insert username of the currently logged in user
        static string activeUser = "A";

        static async Task Main(string[] args)
        {

            //  Register user model
            var regUser = new RegisterUser
            {
                UserName = "A",
                FirstName = "Ase",
                LastName = "Deo",
                Password = "Sallad1.",
                ConfirmPassword = "Sallad1.",
                Country = "Sweden"
            };

            //  Login model
            var authUser = new LoginModel
            {
                Username = "A",
                Password = "Sallad1."
            };

            // Update other user (by username), leave empty if updating own information
            var userToUpdate = "";
            // update model
            var updateUser = new UpdateUserModel
            {
                UserName = "userName",
                FirstName = "firstName",
                LastName = "lastName",
                Password = "password",
                ConfirmPassword = "confirmPassword",
                Country = "country",
                Role = "role"
            };


            //await RegisterUser(regUser);            // Skapa användare
            //await AuthenticateUser(authUser);       // Logga in
            //await UpdateUser(updateUser, userToUpdate);   //   Uppdatera user
            await ViewUsers();                         // Hämta users (admin & VD) 

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
                var register = await client.PostAsJsonAsync("https://localhost:5001/api/user/register", request);
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
            try
            {
                var authenticate = await client.PostAsJsonAsync("https://localhost:5001/api/user/login", request);
                var response = await authenticate.Content.ReadAsAsync<Response>();

                if (authenticate.IsSuccessStatusCode)
                {
                    User user = context.Users.Where(u => u.UserName == request.Username).FirstOrDefault();

                    Token token = new Token
                    {
                        Expires = response.Token.Expires,
                        Payload = response.Token.Payload,
                    };
                    RefreshToken refreshToken = new RefreshToken
                    {
                        Created = response.RefreshToken.Created,
                        Expires = response.RefreshToken.Expires,
                        Token = response.RefreshToken.Token
                    };
                    context.Add(token);
                    context.Add(refreshToken);
                    user.Token = token;
                    user.RefreshToken = refreshToken;
                    context.Update(user);
                    await context.SaveChangesAsync();
                }
                Console.WriteLine(response.Message);
            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }

        private static async Task UpdateUser(UpdateUserModel request, string updateUser = "")
        {
            try
            {
                var user = context.Users.Where(u => u.UserName == activeUser).Include(t=> t.Token).FirstOrDefault();

                if (user.Token.IsExpired)
                {
                    throw new Exception("Token has expiered, refresh token or log in again");
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token.Payload);
                var response = await client.PostAsJsonAsync("https://localhost:5001/api/user/updateuser/" + updateUser, request);

                if (response.IsSuccessStatusCode)
                {
                    if (updateUser != "" && user.UserName != request.UserName)
                    {
                        user.UserName = request.UserName;
                        await context.SaveChangesAsync();
                        Console.WriteLine("User updated successfully");
                    }
                }

            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }

        private static async Task RefreshToken()
        {
            var user = context.Users.Where(u => u.UserName == activeUser).Include(t => t.RefreshToken).FirstOrDefault();

            if (user.RefreshToken.IsExpired)
            {
                throw new Exception("RfreshToken has expiered, log in again");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.RefreshToken.Token);
            var response = await client.PostAsJsonAsync("http://localhost:5001/api/User/RefreshToken", user.RefreshToken.Token);

            if (response.IsSuccessStatusCode)
            {
                var updateToken = await response.Content.ReadAsAsync<AuthenticateResponse>();
                user.RefreshToken = updateToken.RefreshToken;
                user.JwtToken = updateToken.JwtToken;
            }

            return user;
        }

        private static async Task ViewUsers()
        {
            try
            {
                var user = context.Users.Where(u => u.UserName == activeUser).Include(t => t.Token).FirstOrDefault();

                if (user.Token.IsExpired)
                {
                    throw new Exception("Token has expiered, refresh token or log in again");
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token.Payload);
                var response = await client.GetAsync("https://localhost:5001/api/user/GetUsers");

                if (response.IsSuccessStatusCode)
                {
                    var print = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(print);
                }
            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }
    }

}

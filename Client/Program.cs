using Client.Data;
using Client.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        // ACTIVE USER, Insert username of the currently logged in user. DO NOT CHANGE before seed
        static string activeUser = "Admin";

        static async Task Main(string[] args)
        {
            //  Before you start, Remeber to Update-database and then seed

            await Seed();

            //  Register user model(will get role Emplaoyee by default but can be updated in updateuser)
            var regUser = new RegisterUser
            {
                UserName = "KOSA",
                FirstName = "BAasdsseas",
                LastName = "BDsesddo",
                Password = "Sallad1.",
                ConfirmPassword = "Sallad1.",
                Country = "Sweden"
            };

            //  Login model
            var authUser = new LoginModel
            {
                Username = "Admin",
                Password = "Salladskål1."
            };

            // Update other user (by username), leave empty if updating own information. Only admin can do this
            var userToUpdate = "";
            // update model(enter fields you want to update)
            var updateUser = new UpdateUserModel
            {
                UserName = "",
                FirstName = "",
                LastName = "",
                Password = "",
                ConfirmPassword = "",
                Country = "",
                Role = ""
            };

            var deleteUser = new DeleteRequest
            {
                UserName = "B"
            };

            //  Enter employeeID if you want to retrieve employees orders (Only for Admin & VD)
            int? getEmployeesOrders = null;

            //  Enter country to retrieve orders(Can't be empty)
            string country = "USA";


            //  METHODS to call the api

            //await RefreshToken();                     // Refresha Tokens
            //await RegisterUser(regUser);            // Skapa användare
            //await AuthenticateUser(authUser);       // Logga in
            //await UpdateUser(updateUser, userToUpdate);   //   Uppdatera user
            //await DeleteUser(deleteUser);                //  Radera användare
            //await ViewUsers();                         // Hämta users (admin & VD) 
            //await GetMyOrders(getEmployeesOrders);    //  Hämta egna ordrar(samt andras för Vd och admin)
            //await GetCountryOrders(country);          //  Hämta ordrar från ett specifikt land
            //await GetAllOrders();                     //  Hämta alla ordrar



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
                var user = context.Users.Where(u => u.UserName == activeUser).Include(t => t.Token).FirstOrDefault();

                if (user.Token.IsExpired)
                {
                    throw new Exception("Token has expiered, refresh token or log in again");
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token.Payload);
                var response = await client.PutAsJsonAsync("https://localhost:5001/api/user/updateuser/" + updateUser, request);

                if (response.IsSuccessStatusCode)
                {
                    if (updateUser != "" && user.UserName != request.UserName && request.UserName != "")
                    {
                        user.UserName = request.UserName;
                        await context.SaveChangesAsync();

                    }

                    Console.WriteLine("User updated successfully");
                }

            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }

        private static async Task DeleteUser(DeleteRequest request)
        {
            try
            {
                var user = context.Users.Where(u => u.UserName == activeUser).Include(t => t.Token).FirstOrDefault();

                if (user.Token.IsExpired)
                {
                    throw new Exception("Token has expiered, refresh token or log in again");
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token.Payload);
                var response = await client.PostAsJsonAsync("https://localhost:5001/api/user/deleteuser/", request);

                if (response.IsSuccessStatusCode)
                {
                    var deletedUser = context.Users.Where(u => u.UserName == request.UserName).FirstOrDefault();
                    context.Remove(deletedUser);
                    await context.SaveChangesAsync();


                    Console.WriteLine("User has been deleted");
                }

            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }

        private static async Task RefreshToken()
        {
            var user = await context.Users.Where(u => u.UserName == activeUser).Include(t => t.RefreshToken).FirstOrDefaultAsync();

            if (user.RefreshToken.IsExpired)
            {
                throw new Exception("RefreshToken has expiered, log in again");
            }

            var request = new RefreshTokenRequest() { RefreshToken = user.RefreshToken.Token };
            var response = await client.PostAsJsonAsync("https://localhost:5001/api/User/RefreshToken", request);
            var newTokens = await response.Content.ReadAsAsync<Response>();

            if (response.IsSuccessStatusCode)
            {
                Token token = new Token
                {
                    Expires = newTokens.Token.Expires,
                    Payload = newTokens.Token.Payload,
                };
                RefreshToken refreshToken = new RefreshToken
                {
                    Created = newTokens.RefreshToken.Created,
                    Expires = newTokens.RefreshToken.Expires,
                    Token = newTokens.RefreshToken.Token
                };

                user.Token = token;
                user.RefreshToken = refreshToken;
                context.Update(user);
                await context.SaveChangesAsync();
            }

            Console.WriteLine(newTokens.Message);
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
                    var users = await response.Content.ReadAsStringAsync();
                    string[] split = users.Split("},{");
                    foreach (var item in split)
                    {
                        Console.WriteLine($"{item}\n");
                    }
                }
                else
                    Console.WriteLine(response.ReasonPhrase);

            }
            catch
            {
                Console.WriteLine("Something went wrong(OR your token has expired)");
            }
        }

        private static async Task GetMyOrders(int? employeeId)
        {
            var user = context.Users.Where(u => u.UserName == activeUser).Include(t => t.Token).FirstOrDefault();

            if (user.Token.IsExpired)
            {
                throw new ApplicationException("Token has expiered, refresh token or log in again");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token.Payload);
            var response = await client.GetAsync("https://localhost:5001/api/order/GetMyOrders/" + employeeId);

            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadAsStringAsync();
                string[] split = orders.Split("},{");
                foreach (var item in split)
                {
                    Console.WriteLine($"{item}\n\n");
                }

            }

            else
                Console.WriteLine(response.ReasonPhrase);
        }

        private static async Task GetCountryOrders(string country)
        {
            var user = context.Users.Where(u => u.UserName == activeUser).Include(t => t.Token).FirstOrDefault();

            if (user.Token.IsExpired)
            {
                throw new ApplicationException("Token has expiered, refresh token or log in again");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token.Payload);
            var response = await client.GetAsync("https://localhost:5001/api/order/GetCountryOrders/" + country);

            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadAsStringAsync();
                string[] split = orders.Split("},{");
                foreach (var item in split)
                {
                    Console.WriteLine($"{item}\n\n");
                }

            }

            else
                Console.WriteLine(response.ReasonPhrase);
        }

        private static async Task GetAllOrders()
        {
            var user = context.Users.Where(u => u.UserName == activeUser).Include(t => t.Token).FirstOrDefault();

            if (user.Token.IsExpired)
            {
                throw new ApplicationException("Token has expiered, refresh token or log in again");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token.Payload);
            var response = await client.GetAsync("https://localhost:5001/api/order/GetAllOrders/");

            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadAsStringAsync();
                string[] split = orders.Split("},{");
                foreach (var item in split)
                {
                    Console.WriteLine($"{item}\n\n");
                }

            }

            else
                Console.WriteLine(response.ReasonPhrase);
        }

        private static async Task Seed()
        {
            List<RegisterUser> seedUsers = new List<RegisterUser>();
            seedUsers.Add(new RegisterUser
            {
                UserName = "Admin",
                FirstName = "admin",
                LastName = "admin",
                Password = "Salladskål1.",
                ConfirmPassword = "Salladskål1.",
                Country = "Sweden"
            });
            seedUsers.Add(new RegisterUser
            {
                UserName = "Vd",
                FirstName = "Asjsj",
                LastName = "Aoed",
                Password = "Salladskål1.",
                ConfirmPassword = "Salladskål1.",
                Country = "Norway"
            });
            seedUsers.Add(new RegisterUser
            {
                UserName = "CountryManager",
                FirstName = "Base",
                LastName = "Beo",
                Password = "Salladskål1.",
                ConfirmPassword = "Salladskål1.",
                Country = "Finland"
            });
            seedUsers.Add(new RegisterUser
            {
                UserName = "Employee",
                FirstName = "Cisas",
                LastName = "Ciwuw",
                Password = "Salladskål1.",
                ConfirmPassword = "Salladskål1.",
                Country = "France"
            });

            foreach (var user in seedUsers)
            {
                await RegisterUser(user);
            }

            var authUser = new LoginModel
            {
                Username = "Admin",
                Password = "Salladskål1."
            };

            await AuthenticateUser(authUser);

            await UpdateUser(new UpdateUserModel
            {
                UserName = "",
                FirstName = "",
                LastName = "",
                Password = "",
                ConfirmPassword = "",
                Country = "",
                Role = "Vd"
            }, "Vd");
            await UpdateUser(new UpdateUserModel
            {
                UserName = "",
                FirstName = "",
                LastName = "",
                Password = "",
                ConfirmPassword = "",
                Country = "",
                Role = "CountryManager"
            }, "CountryManager");


            Console.WriteLine("Seeding done");

        }
    }
}



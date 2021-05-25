using System;
using System.Collections.Generic;
using System.Text;
using Client.Model;
using Microsoft.EntityFrameworkCore;


namespace Client.Data
{
    public class ClientDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=NorthwindClient;Integrated Security=True");
        }
    }
}

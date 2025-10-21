using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorneoUniversitario.Domain.Entities;

namespace LaTroca.Infrastructure.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDbConnection");
            var databaseName = configuration["DatabaseSettings:DatabaseName"];

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Usuario> Usuarios => _database.GetCollection<Usuario>("users");
        public IMongoCollection<Post> Posts => _database.GetCollection<Post>("posts");

    }
}
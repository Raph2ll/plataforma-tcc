using System;
using System.Collections.Generic;
using client.Db.Repositories.Interfaces;
using client.Models;
using client.Db;
using MySql.Data.MySqlClient;
using Serilog;
using client.Utils;
using System;
using System.Diagnostics;
using System.Reflection;

namespace client.Db.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly DbContext _dbContext;
        private string TableName = "clients";
        private readonly Serilog.ILogger _logger;
        private readonly ContextFactory _ctxFactory;
        private readonly string _namespace = "Repository";


        public ClientRepository(DbContext connection)
        {
            _dbContext = connection;
            _logger = Serilog.Log.ForContext<ClientRepository>();
            _ctxFactory = new ContextFactory(_logger);
        }

        public void CreateClient(Client client)
        {
            var methodName = $"{_namespace} {MethodBase.GetCurrentMethod()!.Name}";

            using (var ctx = _ctxFactory.Create(methodName))
            {
                using (var cmd = new MySqlCommand(@$"INSERT INTO client.{TableName} (id, name, surname, email, birthdate) 
                    VALUES (@Id, @Name, @Surname, @Email, @BirthDate)",
                    _dbContext.Connection))
                {
                    cmd.Parameters.AddWithValue("@Id", client.Id);
                    cmd.Parameters.AddWithValue("@Name", client.Name);
                    cmd.Parameters.AddWithValue("@Surname", client.Surname);
                    cmd.Parameters.AddWithValue("@Email", client.Email);
                    cmd.Parameters.AddWithValue("@BirthDate", client.BirthDate);

                    cmd.ExecuteNonQuery();
                }
            }
        }


        public List<Client> GetClients()
        {
            var methodName = $"{_namespace} {MethodBase.GetCurrentMethod()!.Name}";

            using (var ctx = _ctxFactory.Create(methodName))
            {
                var clients = new List<Client>();

                using (var command = new MySqlCommand($@"SELECT id, name, surname, email, birthdate, created_at, updated_at 
                    FROM client.{TableName} WHERE deleted = false ORDER BY created_at",
                    _dbContext.Connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _logger.Information($"Starting iteration");
                            var client = new Client
                            {
                                Id = reader["id"].ToString(),
                                Name = reader["name"].ToString(),
                                Surname = reader["surname"].ToString(),
                                Email = reader["email"].ToString(),
                                BirthDate = Convert.ToDateTime(reader["birthdate"]),
                                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                UpdatedAt = Convert.ToDateTime(reader["updated_at"])
                            };
                            _logger.Information($"{clients.Count()} Adding a client");

                            clients.Add(client);
                        }
                    }
                }

                return clients;
            }
        }

        public Client GetClientById(string id)
        {
            var methodName = $"{_namespace} {MethodBase.GetCurrentMethod()!.Name}";

            using (var ctx = _ctxFactory.Create(methodName))
            {
                using (var command = new MySqlCommand($"SELECT id, name, surname, email, birthdate, created_at, updated_at FROM client.{TableName} WHERE id = @Id AND deleted = false",
                    _dbContext.Connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Client
                            {
                                Id = reader["id"].ToString(),
                                Name = reader["name"].ToString(),
                                Surname = reader["surname"].ToString(),
                                Email = reader["email"].ToString(),
                                BirthDate = Convert.ToDateTime(reader["birthdate"]),
                                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                UpdatedAt = Convert.ToDateTime(reader["updated_at"])
                            };
                        }
                    }
                }

                return null;
            }
        }

        public void UpdateClient(Client updatedClient)
        {
            var methodName = $"{_namespace} {MethodBase.GetCurrentMethod()!.Name}";

            using (var ctx = _ctxFactory.Create(methodName))
            {
                using (var cmd = new MySqlCommand($"UPDATE client.{TableName} SET name = @Name, surname = @Surname, email = @Email, birthdate = @BirthDate WHERE id = @Id AND deleted = false",
                   _dbContext.Connection))
                {
                    cmd.Parameters.AddWithValue("@Id", updatedClient.Id);
                    cmd.Parameters.AddWithValue("@Name", updatedClient.Name);
                    cmd.Parameters.AddWithValue("@Surname", updatedClient.Surname);
                    cmd.Parameters.AddWithValue("@Email", updatedClient.Email);
                    cmd.Parameters.AddWithValue("@BirthDate", updatedClient.BirthDate);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteClient(string id)
        {
            var methodName = $"{_namespace} {MethodBase.GetCurrentMethod()!.Name}";

            using (var ctx = _ctxFactory.Create(methodName))
            {
                using (var command = new MySqlCommand($"UPDATE client.{TableName} SET deleted = true WHERE id = @Id, deleted = false",
                    _dbContext.Connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
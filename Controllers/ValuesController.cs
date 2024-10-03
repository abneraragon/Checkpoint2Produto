using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net.NetworkInformation;


namespace ProjetoProdutos4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static ConnectionMultiplexer redis;
        public async Task<IActionResult> Get()
        {
            string key = "getusuario";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(10));
            string user = await db.StringGetAsync(key);



            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();

            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(24));
            string userValue = await db.StringGetAsync(key);

            if (!string.IsNullOrEmpty(userValue))
            {
                return Ok(userValue);
            }

            using var connection = new MySqlConnection("Server=localhost;Database=Flap;User=root;Password=123;");
            await connection.OpenAsync();
            string query = @"sekect * from users;";
            var users = await connection.QueryAsync<User>(query);
            string userJson = JsonConvert.SerializeObject(users);
            await db.StringSetAsync(key, userJson);

            return Ok(users);
        }

            [HttpPost]
            public async Task<IActionResult> Post([FromBody] Model.Usuario usuario)
            {
                string connectionString = "Server=localhost;Database=sys;User=root;Password=123;";
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                string sql = @"insert into usuarios(nome, email) 
                            values(@nome, @email);";
                await connection.ExecuteAsync(sql, usuario);

                //apagar o cachê
                string key = "getusuario";
                redis = ConnectionMultiplexer.Connect("localhost:6379");
                IDatabase db = redis.GetDatabase();
                await db.KeyDeleteAsync(key);

                return Ok();
            }

            [HttpPut]
            public async Task<IActionResult> Put([FromBody] Model.Usuario usuario)
            {
                string connectionString = "Server=localhost;Database=sys;User=root;Password=123;";
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                string sql = @"update usuarios 
                            set Nome = @nome, 
	                            Email = @email
                            where Id = @id;";

                await connection.ExecuteAsync(sql, usuario);

                //apagar o cachê
                string key = "getusuario";
                redis = ConnectionMultiplexer.Connect("localhost:6379");
                IDatabase db = redis.GetDatabase();
                await db.KeyDeleteAsync(key);

                return Ok();
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(int id)
            {
                string connectionString = "Server=localhost;Database=sys;User=root;Password=123;";
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                string sql = @"delete from usuarios where Id = @id;";

                await connection.ExecuteAsync(sql, new { id });

                //apagar o cachê
                string key = "getusuario";
                redis = ConnectionMultiplexer.Connect("localhost:6379");
                IDatabase db = redis.GetDatabase();
                await db.KeyDeleteAsync(key);

                return Ok();
            }




        }
    }
}

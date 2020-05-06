using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Dapper;
using Griffin.LoadTestable.Web.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;

namespace Griffin.LoadTestable.Web.Controllers
{
    [ApiController]
    public class LoadController : ControllerBase
    {
        private string _connectionString;
        private readonly TelemetryClient _telemetryClient;

        public LoadController(IConfiguration configuration, TelemetryClient telemetryClient)
        {
            _connectionString = configuration["SqlConnectionString"];
            _telemetryClient = telemetryClient;
        }

        [HttpGet("/allusers")]
        [HttpHead("/allusers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var sqlQuery = @"SELECT [Id]
                          ,[FirstName]
                          ,[LastName]
                          ,[DateOfBirth]
                          ,[EmailAddress]
                      FROM [dbo].[Users]";

            using (var connection = new SqlConnection(_connectionString))
            {
                var users = await connection.QueryAsync<User>(sqlQuery);
                return Ok(users);
            }
        }

        [HttpGet("/alluserspaged")]
        [HttpHead("/alluserspaged")]
        public async Task<IActionResult> GetAllUsersPage([FromQuery]int pageSize = 50, [FromQuery]int pageNumber = 1)
        {
            var sql = @"SELECT TOP(@pageSize) [Id]
                              ,[FirstName]
                              ,[LastName]
                              ,[DateOfBirth]
                              ,[EmailAddress] from
	                          (
                        SELECT ROW_NUmbER() OVER (Order By Id) RowNum, [Id]
                              ,[FirstName]
                              ,[LastName]
                              ,[DateOfBirth]
                              ,[EmailAddress]
                          FROM [dbo].[Users]) seq
                          where seq.RowNum > (@pageNumber * @pageSize)";

            using (var connection = new SqlConnection(_connectionString))
            {
                var users = await connection.QueryAsync<User>(sql, new { pageSize, pageNumber });
                return Ok(users);
            }
        }

        [HttpGet("/allusers/dobbefore")]
        [HttpHead("/allusers/dobbefore")]
        public async Task<IActionResult> GetAllUsersBornBefore([FromQuery]string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return BadRequest("No date provided.");

            var dob = DateTime.Parse(date);

            var sql = @"SELECT [Id]
                          ,[FirstName]
                          ,[LastName]
                          ,[DateOfBirth]
                          ,[EmailAddress]
                      FROM [dbo].[Users]
                      WHERE DateOfBirth < @dob";

            using var connection = new SqlConnection(_connectionString);
            var users = await connection.QueryAsync<User>(sql, new { dob });

            return Ok(users);

        }

        [HttpGet("/allusers/dobafter")]
        [HttpHead("allusers/dobafter")]
        public async Task<IActionResult> GetAllUsersBornAfter([FromQuery]string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return BadRequest("No date provided.");

            var dob = DateTime.Parse(date);

            var sql = @"SELECT [Id]
                          ,[FirstName]
                          ,[LastName]
                          ,[DateOfBirth]
                          ,[EmailAddress]
                      FROM [dbo].[Users]
                      WHERE DateOfBirth > @dob";

            using (var connection = new SqlConnection(_connectionString))
            {
                var users = await connection.QueryAsync<User>(sql, new { dob });
                return Ok(users);
            }
        }
    }
}
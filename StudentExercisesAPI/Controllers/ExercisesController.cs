using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using StudentExercisesAPI.Data;
using Dapper;
using Microsoft.AspNetCore.Http;

namespace StudentExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExercisesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ExercisesController(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET api/students?q=Taco
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string sql = @"
            SELECT
                e.Id,
                e.Name,
                e.Language
            FROM Exercise e
            WHERE 1=1
            ";

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Exercise> exercises = await conn.QueryAsync<Exercise>(
                    sql
                );
                return Ok(exercises);
            }
        }

        // GET api/students/5
        [HttpGet("{id}", Name = "GetExercise")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                e.Id,
                e.Name,
                e.Language
            FROM Exercise e
            WHERE e.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Exercise> exercises = await conn.QueryAsync<Exercise>(sql);
                return Ok(exercises);
            }
        }

        // POST api/students
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Exercise exercise)
        {
            string sql = $@"INSERT INTO Exercise 
            (Name, Language)
            VALUES
            (
                '{exercise.Name}'
                ,'{exercise.Language}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                exercise.Id = newId;
                return CreatedAtRoute("GetExercise", new { id = newId }, exercise);
            }
        }

        // PUT api/students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Exercise exercise)
        {
            string sql = $@"
            UPDATE Exercise
            SET Name = '{exercise.Name}',
                Language = '{exercise.Language}'
            WHERE Id = {id}";

            try
            {
                using (IDbConnection conn = Connection)
                {
                    int rowsAffected = await conn.ExecuteAsync(sql);
                    if (rowsAffected > 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }
                    throw new Exception("No rows affected");
                }
            }
            catch (Exception)
            {
                if (!ExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM Exercise WHERE Id = {id}";

            using (IDbConnection conn = Connection)
            {
                int rowsAffected = await conn.ExecuteAsync(sql);
                if (rowsAffected > 0)
                {
                    return new StatusCodeResult(StatusCodes.Status204NoContent);
                }
                throw new Exception("No rows affected");
            }

        }

        private bool ExerciseExists(int id)
        {
            string sql = $"SELECT Id FROM Exercise WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Exercise>(sql).Count() > 0;
            }
        }
    }

}

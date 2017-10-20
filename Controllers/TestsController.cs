using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PreskriptorAPI.DataAccess;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace PreskriptorAPI.Controllers
{
    [Route("PreskriptorAPI/[controller]")]
    public class TestsController:Controller
    {
        private readonly ILogger<TestsController> _log;
        private readonly ITestsDataAccess _testsDataAccess;
        private IDistributedCache _distributedCache;
        public TestsController(ILogger<TestsController> log, ITestsDataAccess testsDataAccess, IDistributedCache distributedCache)
        {
            _log=log;
            _testsDataAccess=testsDataAccess;
            _distributedCache=distributedCache;
        }

        /// <summary>
        /// Retrieves the list of all tests in the database
        /// </summary>
        /// <response code="200">Test list retrieved.</response>
        /// <response code="404">No tests found in database table.</response>
        /// <response code="500">Server error while retrieving test list.</response>
        [HttpGet]
        [ResponseCache(Duration=30)]
        [ProducesResponseType(typeof(List<Test>),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Get()
        {
            var testList= (List<Test>)null;
            var cacheKey = "TestCache";
            var testCache=_distributedCache.GetString(cacheKey);
            if (!string.IsNullOrWhiteSpace(Convert.ToString(testCache)))
            {
                return Ok(testCache);
            }
            else
            {
                try
                {
                    testList = await _testsDataAccess.GetAllTestsAsync();
                }
                catch (DataAccessException dEx)
                {
                    return StatusCode(500,dEx.Message);
                }
                catch (Exception uEx)
                {
                    return StatusCode(500,uEx.Message);
                }
                var cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(testList),cacheEntryOptions);
                if(testList.Count!=0)
                {
                    return Ok(testList);
                }
                else
                {
                    return NotFound("No tests found in database table.");
                }
                
            }
        }

        /// <summary>
        /// Creates or updates a test in the database
        /// </summary>
        /// <response code="201">Test created/updated.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="500">Server error while creating/updating test.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Test),201)]
        [ProducesResponseType(typeof(string),400)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Post([FromBody]Test test)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    await _testsDataAccess.SaveTestAsync(test);
                }
                catch (JsonException JEX)
                {
                    return BadRequest(JEX.Message);
                }
                catch (DataAccessException DAX)
                {
                    return StatusCode(500, DAX.Message);
                }
                catch (Exception EX)
                {
                    return StatusCode(500, EX.Message);
                }
                return Created("",test);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Retrieves the details of a test given the type
        /// </summary>
        /// <response code="200">Test details retrieved.</response>
        /// <response code="404">Test not found in database table.</response>
        /// <response code="500">Server error while retrieving test details.</response>
        [HttpGet("{type}")]
        [ResponseCache(Duration=30)]
        [ProducesResponseType(typeof(Test),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Get(string type)
        {
            var test= (Test)null;
            
            try
            {
                test = await _testsDataAccess.GetTestAsync(type);
            }
            catch (DataAccessException dEx)
            {
                return StatusCode(500,dEx.Message);
            }
            catch (Exception uEx)
            {
                return StatusCode(500,uEx.Message);
            }
            if(test!=null)
            {
                return Ok(test);
            }
            else
            {
                return NotFound("Test not found in database table.");
            }
        }

        /// <summary>
        /// Deletes a test given the type
        /// </summary>
        /// <response code="204">Test deleted.</response>
        /// <response code="500">Server error while deleting test.</response>
        [HttpDelete("{type}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Delete(string type)
        {
            try
            {
               await _testsDataAccess.DeleteTestAsync(type);
            }
            catch (DataAccessException dEx)
            {
                return StatusCode(500,dEx.Message);
            }
            catch (Exception uEx)
            {
                return StatusCode(500,uEx.Message);
            }    
            return NoContent();
        }
        
        /// <summary>
        /// Retrieves a list of all test types from the database
        /// </summary>
        /// <response code="200">Test types retrieved.</response>
        /// <response code="404">No tests found in database table.</response>
        /// <response code="500">Server error while retrieving test types.</response>
        [HttpGet("Types")]
        [ProducesResponseType(typeof(List<string>),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Types()
        {
            var typeList = (List<string>)null;
            var cacheKey = "TestTypeCache";
            var testTypeCache=_distributedCache.GetString(cacheKey);

            if (!string.IsNullOrWhiteSpace(Convert.ToString(testTypeCache)))
            {
                return Ok(testTypeCache);
            }
            else
            {
                try
                {
                    typeList = await _testsDataAccess.GetAllTypesAsync();
                }
                catch (DataAccessException dEx)
                {
                    return StatusCode(500,dEx.Message);
                }
                catch (Exception uEx)
                {
                    return StatusCode(500,uEx.Message);
                }
                var cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(typeList),cacheEntryOptions);
                if(typeList.Count!=0)
                {
                    return Ok(typeList);
                }
                else
                {
                    return NotFound("No tests found in database table.");
                }
            }
        }
    }
}
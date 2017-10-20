using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PreskriptorAPI.DataAccess;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PreskriptorAPI.Controllers
{
    [Route("PreskriptorAPI/[controller]")]
    public class LetterheadsController:Controller
    {
        private readonly ILogger<LetterheadsController> _log;
        private readonly ILetterheadsDataAccess _letterheadsDataAccess;
        private IDistributedCache _distributedCache;
        public LetterheadsController(ILogger<LetterheadsController> log, ILetterheadsDataAccess letterheadsDataAccess, IDistributedCache distributedCache)
        {
            _log=log;
            _letterheadsDataAccess=letterheadsDataAccess;
            _distributedCache=distributedCache;
        }

        /// <summary>
        /// Retrieves the list of all letterheads in the database
        /// </summary>
        /// <response code="200">Letterhead list retrieved.</response>
        /// <response code="404">No letterheads found in database table.</response>
        /// <response code="500">Server error while retrieving letterhead list.</response>
        [HttpGet]
        [ResponseCache(Duration=30)]
        [ProducesResponseType(typeof(List<Letterhead>),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Get()
        {
            var letterheadList= (List<Letterhead>)null;
            var cacheKey = "LetterheadCache";
            var letterheadCache=_distributedCache.GetString(cacheKey);
            if (!string.IsNullOrWhiteSpace(Convert.ToString(letterheadCache)))
            {
                return Ok(letterheadCache);
            }
            else
            {
                try
                {
                    letterheadList = await _letterheadsDataAccess.GetAllLetterheadsAsync();
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
                _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(letterheadList, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver()}),cacheEntryOptions);
                if(letterheadList.Count!=0)
                {
                    return Ok(letterheadList);
                }
                else
                {
                    return NotFound("No letterheads found in database table.");
                }
                
            }
        }

        /// <summary>
        /// Creates or updates a letterhead in the database
        /// </summary>
        /// <response code="201">Letterhead created/updated.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="500">Server error while creating/updating letterhead.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Letterhead),201)]
        [ProducesResponseType(typeof(string),400)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Post([FromBody]Letterhead letterhead)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    await _letterheadsDataAccess.SaveLetterheadAsync(letterhead);
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
                return Created("",letterhead);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Retrieves the details of a letterhead given the chamber name
        /// </summary>
        /// <response code="200">Letterhead details retrieved.</response>
        /// <response code="404">Letterhead not found in database table.</response>
        /// <response code="500">Server error while retrieving letterhead details.</response>
        [HttpGet("{chamber_name}")]
        [ResponseCache(Duration=30)]
        [ProducesResponseType(typeof(Letterhead),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Get(string chamber_name)
        {
            var letterhead= (Letterhead)null;
            
            try
            {
                letterhead = await _letterheadsDataAccess.GetLetterheadAsync(chamber_name);
            }
            catch (DataAccessException dEx)
            {
                return StatusCode(500,dEx.Message);
            }
            catch (Exception uEx)
            {
                return StatusCode(500,uEx.Message);
            }
            if(letterhead!=null)
            {
                return Ok(letterhead);
            }
            else
            {
                return NotFound("Letterhead not found in database table.");
            }
        }

        /// <summary>
        /// Deletes a letterhead given the chamber name
        /// </summary>
        /// <response code="204">Letterhead deleted.</response>
        /// <response code="500">Server error while deleting letterhead.</response>
        [HttpDelete("{chamber_name}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Delete(string chamber_name)
        {
            try
            {
               await _letterheadsDataAccess.DeleteLetterheadAsync(chamber_name);
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
        /// Retrieves a list of all chamber names from the database
        /// </summary>
        /// <response code="200">Chamber names retrieved.</response>
        /// <response code="404">No letterheads found in database table.</response>
        /// <response code="500">Server error while retrieving chamber names.</response>
        [HttpGet("Chamber-Names")]
        [ProducesResponseType(typeof(List<string>),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> ChamberNames()
        {
            var chamberNameList = (List<string>)null;
            var cacheKey = "ChamberNameCache";
            var chamberNameCache=_distributedCache.GetString(cacheKey);

            if (!string.IsNullOrWhiteSpace(Convert.ToString(chamberNameCache)))
            {
                return Ok(chamberNameCache);
            }
            else
            {
                try
                {
                    chamberNameList = await _letterheadsDataAccess.GetAllChamberNamesAsync();
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
                _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(chamberNameList),cacheEntryOptions);
                if(chamberNameList.Count!=0)
                {
                    return Ok(chamberNameList);
                }
                else
                {
                    return NotFound("No letterheads found in database table.");
                }
            }
        }
    }
}
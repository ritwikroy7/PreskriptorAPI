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
    public class DrugsController:Controller
    {
        private readonly ILogger<DrugsController> _log;
        private readonly IDrugsDataAccess _drugsDataAccess;
        private IDistributedCache _distributedCache;
        public DrugsController(ILogger<DrugsController> log, IDrugsDataAccess drugsDataAccess, IDistributedCache distributedCache)
        {
            _log=log;
            _drugsDataAccess=drugsDataAccess;
            _distributedCache=distributedCache;
        }

        /// <summary>
        /// Retrieves the list of all drugs in the database
        /// </summary>
        /// <response code="200">Drug list retrieved.</response>
        /// <response code="404">No drugs found in database table.</response>
        /// <response code="500">Server error while retrieving drug list.</response>
        [HttpGet]
        [ResponseCache(Duration=30)]
        [ProducesResponseType(typeof(List<Drug>),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Get()
        {
            var drugList= (List<Drug>)null;
            var cacheKey = "DrugCache";
            var drugCache=_distributedCache.GetString(cacheKey);
            if (!string.IsNullOrWhiteSpace(Convert.ToString(drugCache)))
            {
                return Ok(drugCache);
            }
            else
            {
                try
                {
                    drugList = await _drugsDataAccess.GetAllDrugsAsync();
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
                _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(drugList),cacheEntryOptions);
                if(drugList.Count!=0)
                {
                    return Ok(drugList);
                }
                else
                {
                    return NotFound("No drugs found in database table.");
                }
                
            }
        }

        /// <summary>
        /// Creates or updates a drug in the database
        /// </summary>
        /// <response code="201">Drug created/updated.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="500">Server error while creating/updating drug.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Drug),201)]
        [ProducesResponseType(typeof(string),400)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Post([FromBody]Drug drug)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    await _drugsDataAccess.SaveDrugAsync(drug);
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
                return Created("",drug);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Retrieves the details of a drug given the trade name
        /// </summary>
        /// <response code="200">Drug details retrieved.</response>
        /// <response code="404">Drug not found in database table.</response>
        /// <response code="500">Server error while retrieving drug details.</response>
        [HttpGet("{trade_name}")]
        [ResponseCache(Duration=30)]
        [ProducesResponseType(typeof(Drug),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Get(string trade_name)
        {
            var drug= (Drug)null;
            
            try
            {
                drug = await _drugsDataAccess.GetDrugAsync(trade_name);
            }
            catch (DataAccessException dEx)
            {
                return StatusCode(500,dEx.Message);
            }
            catch (Exception uEx)
            {
                return StatusCode(500,uEx.Message);
            }
            if(drug!=null)
            {
                return Ok(drug);
            }
            else
            {
                return NotFound("Drug not found in database table.");
            }
        }

        /// <summary>
        /// Deletes a drug given the trade name
        /// </summary>
        /// <response code="204">Drug deleted.</response>
        /// <response code="500">Server error while deleting drug.</response>
        [HttpDelete("{trade_name}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Delete(string trade_name)
        {
            try
            {
               await _drugsDataAccess.DeleteDrugAsync(trade_name);
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
        /// Retrieves a list of all drug trade names from the database
        /// </summary>
        /// <response code="200">Trade names retrieved.</response>
        /// <response code="404">No drugs found in database table.</response>
        /// <response code="500">Server error while retrieving trade names.</response>
        [HttpGet("Trade-Names")]
        [ProducesResponseType(typeof(List<string>),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> TradeNames()
        {
            var tradeNameList = (List<string>)null;
            var cacheKey = "TradeNameCache";
            var tradeNameCache=_distributedCache.GetString(cacheKey);

            if (!string.IsNullOrWhiteSpace(Convert.ToString(tradeNameCache)))
            {
                return Ok(tradeNameCache);
            }
            else
            {
                try
                {
                    tradeNameList = await _drugsDataAccess.GetAllTradeNamesAsync();
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
                _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(tradeNameList),cacheEntryOptions);
                if(tradeNameList.Count!=0)
                {
                    return Ok(tradeNameList);
                }
                else
                {
                    return NotFound("No drugs found in database table.");
                }
            }
        }
    }
}
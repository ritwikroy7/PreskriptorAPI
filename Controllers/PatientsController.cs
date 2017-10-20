using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PreskriptorAPI.DataAccess;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Linq;

namespace PreskriptorAPI.Controllers
{
    [Route("PreskriptorAPI/[controller]")]
    public class PatientsController:Controller
    {
        private readonly ILogger<PatientsController> _log;
        private readonly IPatientsDataAccess _patientsDataAccess;
        private IDistributedCache _distributedCache;
        public PatientsController(ILogger<PatientsController> log, IPatientsDataAccess patientsDataAccess, IDistributedCache distributedCache)
        {
            _log=log;
            _patientsDataAccess=patientsDataAccess;
            _distributedCache=distributedCache;
        }

        /// <summary>
        /// Retrieves the list of all patients in the database
        /// </summary>
        /// <response code="200">Patient list retrieved.</response>
        /// <response code="404">No patients found in database table.</response>
        /// <response code="500">Server error while retrieving patient list.</response>
        [HttpGet]
        [ResponseCache(Duration=30)]
        [ProducesResponseType(typeof(List<Patient>),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Get()
        {
            var patientList= (List<Patient>)null;
            var cacheKey = "PatientCache";
            var patientCache=_distributedCache.GetString(cacheKey);
            if (!string.IsNullOrWhiteSpace(Convert.ToString(patientCache)))
            {
                return Ok(patientCache);
            }
            else
            {
                try
                {
                    patientList = await _patientsDataAccess.GetAllPatientsAsync();
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
                _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(patientList),cacheEntryOptions);
                if(patientList.Count!=0)
                {
                    return Ok(patientList);
                }
                else
                {
                    return NotFound("No patients found in database table.");
                }
                
            }
        }

        /// <summary>
        /// Creates or updates a patient in the database
        /// </summary>
        /// <response code="201">Patient created/updated.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="500">Server error while creating/updating patient.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Patient),201)]
        [ProducesResponseType(typeof(string),400)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Post([FromBody]Patient patient)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    await _patientsDataAccess.SavePatientAsync(patient);
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
                return Created("",patient);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Retrieves the details of a patient given the patient ID
        /// </summary>
        /// <response code="200">Patient details retrieved.</response>
        /// <response code="404">Patient not found in database table.</response>
        /// <response code="500">Server error while retrieving patient details.</response>
        [HttpGet("{patient_id}")]
        [ResponseCache(Duration=30)]
        [ProducesResponseType(typeof(Patient),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Get(string patient_id)
        {
            var patient= (Patient)null;
            
            try
            {
                patient = await _patientsDataAccess.GetPatientAsync(patient_id);
            }
            catch (DataAccessException dEx)
            {
                return StatusCode(500,dEx.Message);
            }
            catch (Exception uEx)
            {
                return StatusCode(500,uEx.Message);
            }
            if(patient!=null)
            {
                return Ok(patient);
            }
            else
            {
                return NotFound("Patient not found in database table.");
            }
        }

        /// <summary>
        /// Deletes a patient given the patient ID
        /// </summary>
        /// <response code="204">Patient deleted.</response>
        /// <response code="500">Server error while deleting patient.</response>
        [HttpDelete("{patient_id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Delete(string patient_id)
        {
            try
            {
               await _patientsDataAccess.DeletePatientAsync(patient_id);
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
        /// Retrieves a list of all patient names from the database
        /// </summary>
        /// <response code="200">Patient names retrieved.</response>
        /// <response code="404">No patients found in database table.</response>
        /// <response code="500">Server error while retrieving patient names.</response>
        [HttpGet("Patient-Names")]
        [ProducesResponseType(typeof(List<string>),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> PatientNames()
        {
            var patientNameList = (List<string>)null;
            var cacheKey = "PatientNameCache";
            var patientNameCache=_distributedCache.GetString(cacheKey);

            if (!string.IsNullOrWhiteSpace(Convert.ToString(patientNameCache)))
            {
                return Ok(patientNameCache);
            }
            else
            {
                try
                {
                    patientNameList = await _patientsDataAccess.GetAllPatientNamesAsync();
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
                _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(patientNameList),cacheEntryOptions);
                if(patientNameList.Count!=0)
                {
                    return Ok(patientNameList.Distinct());
                }
                else
                {
                    return NotFound("No patients found in database table.");
                }
            }
        }

        /// <summary>
        /// Retrieves a patient(s) given the patient name
        /// </summary>
        /// <response code="200">Patient(s) retrieved.</response>
        /// <response code="404">Patient not found in database table.</response>
        /// <response code="500">Server error while retrieving patient(s).</response>
        [HttpGet("Search/{patient_name}")]
        [ResponseCache(Duration=30)]
        [ProducesResponseType(typeof(List<Patient>),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Search(string patient_name)
        {
            var listPatient= (List<Patient>)null;
            
            try
            {
                listPatient = await _patientsDataAccess.SearchPatientAsync(patient_name);
            }
            catch (DataAccessException dEx)
            {
                return StatusCode(500,dEx.Message);
            }
            catch (Exception uEx)
            {
                return StatusCode(500,uEx.Message);
            }
            if(listPatient.Count!=0)
            {
                return Ok(listPatient);
            }
            else
            {
                return NotFound("Patient not found in database table.");
            }
        }

        /// <summary>
        /// Retrieves the total number of patients in the database
        /// </summary>
        /// <response code="200">Patient count retrieved.</response>
        /// <response code="500">Server error while retrieving patient count.</response>
        [HttpGet("Count")]
        [ResponseCache(Duration=30)]
        [ProducesResponseType(typeof(int),200)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Count()
        {
            long patienttCount = 0;
            
            try
            {
                patienttCount = await _patientsDataAccess.GetPatientCountAsync();
            }
            catch (DataAccessException dEx)
            {
                return StatusCode(500,dEx.Message);
            }
            catch (Exception uEx)
            {
                return StatusCode(500,uEx.Message);
            }
            return Ok(patienttCount);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PreskriptorAPI.DataAccess;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using PreskriptorAPI.PDFGenerator;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Globalization;

namespace PreskriptorAPI.Controllers
{
    [Route("PreskriptorAPI/[controller]")]
    public class PrescriptionsController:Controller
    {
        private readonly ILogger<PrescriptionsController> _log;
        private readonly IPrescriptionsDataAccess _prescriptionsDataAccess;
        private readonly IDrugsDataAccess _drugsDataAccess;
        private readonly ILetterheadsDataAccess _letterheadsDataAccess;
        private readonly IPrescriptionPDFGenerator _prescriptionPDFGenerator;
        private IDistributedCache _distributedCache;
        public PrescriptionsController(ILogger<PrescriptionsController> log, IPrescriptionsDataAccess prescriptionsDataAccess, IDrugsDataAccess drugsDataAccess, ILetterheadsDataAccess letterheadsDataAccess, IPrescriptionPDFGenerator prescriptionPDFGenerator, IDistributedCache distributedCache)
        {
            _log=log;
            _prescriptionsDataAccess=prescriptionsDataAccess;
            _drugsDataAccess = drugsDataAccess;
            _letterheadsDataAccess = letterheadsDataAccess;
            _distributedCache=distributedCache;
            _prescriptionPDFGenerator=prescriptionPDFGenerator;
            
        }

        /// <summary>
        /// Creates or updates a prescription in the database
        /// </summary>
        /// <response code="201">Prescription created/updated.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="500">Server error while creating/updating prescription.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Prescription),201)]
        [ProducesResponseType(typeof(string),400)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Post([FromBody]Prescription prescription)
        {
            if(ModelState.IsValid)
            {   
                try
                {
                    if(String.IsNullOrWhiteSpace(prescription.PrescriptionID))
                    {
                        prescription.PrescriptionID=Convert.ToString(Guid.NewGuid());
                        prescription.PatientInfo.PatientID=prescription.PrescriptionID;
                    }

                    if(prescription.Medications!=null && prescription.Medications.Count>0)
                    {
                        var drug=(Drug)null;
                        foreach(Medication medication in prescription.Medications)
                        {
                            medication.Composition = new List<string>();
                            drug = new Drug();
                            drug =  await _drugsDataAccess.GetDrugAsync(medication.TradeName);
                            if(drug!=null)
                            {
                                medication.Composition=drug.Composition;
                            }
                        }  
                    }
                    
                    if(prescription.Letterhead!=null && (!(String.IsNullOrWhiteSpace(prescription.Letterhead.ChamberName))))
                    {
                        var letterhead = (Letterhead)null;
                        letterhead=await _letterheadsDataAccess.GetLetterheadAsync(prescription.Letterhead.ChamberName);
                        if(letterhead!=null)
                        {
                            prescription.Letterhead=letterhead;
                        }
                    }

                    await _prescriptionsDataAccess.SavePrescriptionAsync(prescription);
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
                return Created("",prescription);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Prints the details of a prescription given the prescription ID
        /// </summary>
        /// <response code="200">Prescription details retrieved.</response>
        /// <response code="404">Prescription not found in database table.</response>
        /// <response code="500">Server error while retrieving prescription details.</response>
        [HttpGet("{prescription_id}")]
        [ResponseCache(Duration=30)]
        [ProducesResponseType(typeof(Prescription),200)]
        [ProducesResponseType(typeof(string),404)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> Get(string prescription_id)
        {
            var prescription= (Prescription)null;
            
            try
            {
                prescription = await _prescriptionsDataAccess.GetPrescriptionAsync(prescription_id);
            }
            catch (DataAccessException dEx)
            {
                return StatusCode(500,dEx.Message);
            }
            catch (Exception uEx)
            {
                return StatusCode(500,uEx.Message);
            }
            if(prescription!=null)
            {
                return Ok(prescription);
            }
            else
            {
                return NotFound("Prescription not found in database table.");
            }
        }

        /// <summary>
        /// Creates or updates a prescription in the database and then generates a PDF file from the contents of the prescription
        /// </summary>
        /// <response code="200">Prescription PDF generated successfully.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="500">Server error while generating prescription PDF.</response>
        [HttpPost("PDF")]
        [ProducesResponseType(typeof(FileContentResult),200)]
        [ProducesResponseType(typeof(string),400)]
        [ProducesResponseType(typeof(string),500)]
        public async Task<IActionResult> PDF([FromBody]Prescription prescription)
        {
            var prescriptionPDF=(byte[])null;
            if(ModelState.IsValid)
            {   
                try
                {
                    if(String.IsNullOrWhiteSpace(prescription.PrescriptionID))
                    {
                        prescription.PrescriptionID=Convert.ToString(new Guid());
                        prescription.PatientInfo.PatientID=prescription.PrescriptionID;
                    }

                    if(prescription.Medications!=null && prescription.Medications.Count>0)
                    {
                        var drug=(Drug)null;
                        foreach(Medication medication in prescription.Medications)
                        {
                            medication.Composition = new List<string>();
                            drug = new Drug();
                            drug =  await _drugsDataAccess.GetDrugAsync(medication.TradeName);
                            if(drug.Composition!=null)
                            {
                                medication.Composition=drug.Composition;
                            }
                        }  
                    }
                    
                    if(prescription.Letterhead!=null && (!(String.IsNullOrWhiteSpace(prescription.Letterhead.ChamberName))))
                    {
                        var letterhead = (Letterhead)null;
                        letterhead=await _letterheadsDataAccess.GetLetterheadAsync(prescription.Letterhead.ChamberName);
                        prescription.Letterhead=letterhead;
                    }

                    await _prescriptionsDataAccess.SavePrescriptionAsync(prescription);
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
                try
                {
                    prescriptionPDF=_prescriptionPDFGenerator.GeneratePDF(prescription);
                }
                catch(PDFGeneratorException PGEX)
                {
                    return StatusCode(500, PGEX.Message);
                }
                catch (Exception Ex)
                {
                    return StatusCode(500, Ex.Message);
                }
                StringValues requestHeaderValues;
                if(Request.Headers.TryGetValue("PDFReturnFormat",out requestHeaderValues))
                {
                    if(String.Equals(requestHeaderValues.FirstOrDefault(),"Base64", StringComparison.OrdinalIgnoreCase))
                    {
                        var prescriptionPDFBase64 = Convert.ToBase64String(prescriptionPDF);
                        return Ok(prescriptionPDFBase64);
                    }
                }
                return File(prescriptionPDF, "application/pdf",prescription.PrescriptionID+".pdf");
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
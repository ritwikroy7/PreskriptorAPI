using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using System.Threading;
using Amazon.Runtime;
using Microsoft.Extensions.Logging;
using System;
using Newtonsoft.Json;
using Amazon.DynamoDBv2.Model;
using PreskriptorAPI.DataAccess;


namespace PreskriptorAPI.DataAccess
{
    public interface IPrescriptionsDataAccess
    {
        Task SavePrescriptionAsync(Prescription prescription);
        Task<Prescription> GetPrescriptionAsync(string prescriptionID);
    }
    public class PrescriptionsDataAccess:IPrescriptionsDataAccess
    {
        private readonly ILogger<PrescriptionsDataAccess> _log;
        private readonly IPatientsDataAccess _patientsDataAccess;
        
        public PrescriptionsDataAccess(ILogger<PrescriptionsDataAccess> log, IPatientsDataAccess patientsDataAccess)
        {
            _log=log;
            _patientsDataAccess=patientsDataAccess;
        }
        
        public async Task SavePrescriptionAsync(Prescription prescription)
        {
            var _prescriptionJson = (string)null;
            try
            {
                _prescriptionJson=JsonConvert.SerializeObject(prescription);
            }
            catch(JsonException jEx)
            {
                throw jEx;
            }
            Document document=null;
            var patient = prescription.PatientInfo;
            try
            {
                await _patientsDataAccess.SavePatientAsync(patient);
            }
            catch (DataAccessException DAX)
            {
                throw DAX;
            }
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"Prescriptions");
                    var item = Document.FromJson(_prescriptionJson);
                    document = await table.PutItemAsync(item,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Saving Prescription To Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Saving Prescription To Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Saving Prescription To Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
        public async Task<Prescription> GetPrescriptionAsync(string prescriptionID)
        {
            var _prescription = (Document)null;
            var _prescriptionJson = (string)null;
            Prescription prescription = null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"Prescriptions");
                    _prescription = await table.GetItemAsync(prescriptionID,default(CancellationToken));
                    _prescriptionJson = _prescription.ToJson();
                }
                try
                {
                    prescription=JsonConvert.DeserializeObject<Prescription>(_prescriptionJson);
                }
                catch(JsonException jEx)
                {
                    throw jEx;
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Prescription From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Prescription From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Prescription From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return prescription;
        }


    }
}
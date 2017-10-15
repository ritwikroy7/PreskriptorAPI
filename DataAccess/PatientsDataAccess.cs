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

namespace PreskriptorAPI.DataAccess
{
    public interface IPatientsDataAccess
    {
        Task<List<Patient>> GetAllPatientsAsync();
        Task SavePatientAsync(Patient patient);
        Task<Patient> GetPatientAsync(string patientID);
        Task DeletePatientAsync(string patientID);
        Task<List<string>> GetAllPatientNamesAsync();
        Task<List<Patient>> SearchPatientAsync(string patientName);
        Task<long> GetPatientCountAsync();
    }
    
    public class PatientsDataAccess:IPatientsDataAccess
    {
        private readonly ILogger<PatientsDataAccess> _log;
        public PatientsDataAccess(ILogger<PatientsDataAccess> log)
        {
            _log=log;
        }
        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            List<Patient> patientList = new List<Patient>();
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"Patients");
                    ScanFilter scanFilter = new ScanFilter();
                    Search search = table.Scan(scanFilter);
                    List<Document> documentList = new List<Document>();
                    do
                    {
                        documentList=await search.GetNextSetAsync(default(CancellationToken));
                        foreach(var document in documentList)
                        {
                            Patient patient = new Patient();
                            try
                            {
                                patient=JsonConvert.DeserializeObject<Patient>(document.ToJson());
                                patientList.Add(patient);
                            }
                            catch(JsonException jEx)
                            {
                                _log.LogError("Json Deserialization Exception: "+jEx.Message);
                                throw new DataAccessException("An Error Occured While Retrieving Patient List From Database");
                            }
                        }
                    } while(!search.IsDone);
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient List From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient List From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient List From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return patientList;
        }
        public async Task SavePatientAsync(Patient patient)
        {
            var _patientJson = (string)null;
            try
            {
                _patientJson=JsonConvert.SerializeObject(patient);
            }
            catch(JsonException jEx)
            {
                throw jEx;
            }
            Document document=null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"Patients");
                    var item = Document.FromJson(_patientJson);
                    document = await table.PutItemAsync(item,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Saving Patient To Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Saving Patient To Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Saving Patient To Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
        public async Task<Patient> GetPatientAsync(string patientID)
        {
            var _patient = (Document)null;
            var _patientJson = (string)null;
            Patient patient = null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"Patients");
                    _patient = await table.GetItemAsync(patientID,default(CancellationToken));
                    if(_patient!=null)
                    {
                        _patientJson = _patient.ToJson();
                    }
                }
                try
                {
                    patient=JsonConvert.DeserializeObject<Patient>(_patientJson);
                }
                catch(JsonException jEx)
                {
                    _log.LogError("Json Deserialization Exception: "+jEx.Message);
                    throw new DataAccessException("An Error Occured While Retrieving Patient From Database");
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return patient;
        }
        public async Task DeletePatientAsync(string patientID)
        {
            Document document=null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"Patients");
                    document = await table.DeleteItemAsync(patientID,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Patient From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Patient From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Patient From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
        public async Task<List<string>> GetAllPatientNamesAsync()
        {
            List<string> patientNameList = new List<string>();
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"Patients");
                    ScanFilter scanFilter = new ScanFilter();
                    ScanOperationConfig scanConfig = new ScanOperationConfig()
                    {
                        Select = SelectValues.SpecificAttributes,
                        AttributesToGet = new List<string> { "PatientName" },
                        Filter = scanFilter
                    };
                    Search search = table.Scan(scanConfig);
                    List<Document> documentList = new List<Document>();
                    do
                    {
                        documentList=await search.GetNextSetAsync(default(CancellationToken));
                        foreach(var document in documentList)
                        {
                            var patientName=document["PatientName"];
                            patientNameList.Add(patientName);
                        }
                    } while(!search.IsDone);
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Names From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Names From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Names From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return patientNameList;
        }
        public async Task<List<Patient>> SearchPatientAsync(string patientName)
        {
            List<Patient> patientList = new List<Patient>();
            QueryResponse queryResponse=null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    QueryRequest queryRequest = new QueryRequest
                    {
                        TableName = "Patients",
                        IndexName = "PatientName-Index",
                        KeyConditionExpression = "PatientName = :v_PatientName",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                            {":v_PatientName", new AttributeValue { S =  patientName }}
                        },
                        ScanIndexForward = true
                    };
                    queryResponse = await dynamoClient.QueryAsync(queryRequest,default(CancellationToken));
                    if(queryResponse.Items.Count!=0)
                    {
                        foreach(Dictionary<string, AttributeValue> dynamoItem in queryResponse.Items)
                        {
                            Patient patient = new Patient();
                            patient.PatientName=Convert.ToString(dynamoItem["PatientName"].S);
                            patient.PatientID=Convert.ToString(dynamoItem["PatientID"].S);
                            
                            if(dynamoItem.ContainsKey("Age"))
                            {
                                patient.Age=Convert.ToInt32(dynamoItem["Age"].N);
                            }
                            if(dynamoItem.ContainsKey("ContactNumber"))
                            {
                                patient.ContactNumber=Convert.ToString(dynamoItem["ContactNumber"].S);
                            }
                            if(dynamoItem.ContainsKey("Parity"))
                            {
                                patient.Parity=Convert.ToString(dynamoItem["Parity"].S);
                            }
                            if(dynamoItem.ContainsKey("BloodGroup"))
                            {
                                patient.BloodGroup=Convert.ToString(dynamoItem["BloodGroup"].S);
                            }
                            if(dynamoItem.ContainsKey("Title"))
                            {
                                patient.Title=Convert.ToString(dynamoItem["Title"].S);
                            }
                            if(dynamoItem.ContainsKey("Email"))
                            {
                                patient.Email=Convert.ToString(dynamoItem["Email"].S);
                            }
                            patientList.Add(patient);
                        }
                    }
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient(s) From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient(s) From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient(s) From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return patientList;
        }
        public async Task<long> GetPatientCountAsync()
        {
            long patientCount=0;

            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var request = new DescribeTableRequest("Patients");
                    var response = dynamoClient.DescribeTableAsync(request);
                    patientCount = response.Result.Table.ItemCount;
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Count From Database"); 
            }
            catch (AmazonServiceException aEx)
            {
                _log.LogError("Amazon Service Exception: "+aEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Count From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Count From Database");
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception:  "+eEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return patientCount;
        }

    }
}
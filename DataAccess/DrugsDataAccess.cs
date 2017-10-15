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
    public interface IDrugsDataAccess
    {
        Task<List<Drug>> GetAllDrugsAsync();
        Task SaveDrugAsync(Drug drug);
        Task<Drug> GetDrugAsync(string tradeName);
        Task DeleteDrugAsync(string tradeName);
        Task<List<string>> GetAllTradeNamesAsync();
    }
    
    public class DrugsDataAccess:IDrugsDataAccess
    {
        private readonly ILogger<DrugsDataAccess> _log;
        public DrugsDataAccess(ILogger<DrugsDataAccess> log)
        {
            _log=log;
        }
        public async Task<List<Drug>> GetAllDrugsAsync()
        {
            List<Drug> drugList = new List<Drug>();
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"DrugMaster");
                    ScanFilter scanFilter = new ScanFilter();
                    Search search = table.Scan(scanFilter);
                    List<Document> documentList = new List<Document>();
                    do
                    {
                        documentList=await search.GetNextSetAsync(default(CancellationToken));
                        foreach(var document in documentList)
                        {
                            Drug drug = new Drug();
                            try
                            {
                                drug=JsonConvert.DeserializeObject<Drug>(document.ToJson());
                                drugList.Add(drug);
                            }
                            catch(JsonException jEx)
                            {
                                _log.LogError("Json Deserialization Exception: "+jEx.Message);
                                throw new DataAccessException("An Error Occured While Retrieving Drug List From Database");
                            }
                        }
                    } while(!search.IsDone);
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug List From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug List From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug List From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return drugList;
        }
        public async Task SaveDrugAsync(Drug drug)
        {
            var _drugJson = (string)null;
            try
            {
                _drugJson=JsonConvert.SerializeObject(drug);
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
                    var table = Table.LoadTable(dynamoClient,"DrugMaster");
                    var item = Document.FromJson(_drugJson);
                    document = await table.PutItemAsync(item,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Saving Drug To Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Saving Drug To Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Saving Drug To Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
        public async Task<Drug> GetDrugAsync(string tradeName)
        {
            var _drug = (Document)null;
            var _drugJson = (string)null;
            Drug drug = null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"DrugMaster");
                    _drug = await table.GetItemAsync(tradeName,default(CancellationToken));
                    if(_drug!=null)
                    {
                        _drugJson = _drug.ToJson();
                    }
                }
                if(_drugJson!=null)
                {
                    try
                    {
                        drug=JsonConvert.DeserializeObject<Drug>(_drugJson);
                    }
                    catch(JsonException jEx)
                    {
                        _log.LogError("Json Deserialization Exception: "+jEx.Message);
                        throw new DataAccessException("An Error Occured While Retrieving Drug From Database");
                    }
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return drug;
        }
        public async Task DeleteDrugAsync(string tradeName)
        {
            Document document=null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"DrugMaster");
                    document = await table.DeleteItemAsync(tradeName,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Drug From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Drug From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Drug From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
        public async Task<List<string>> GetAllTradeNamesAsync()
        {
            List<string> tradeNameList = new List<string>();
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"DrugMaster");
                    ScanFilter scanFilter = new ScanFilter();
                    ScanOperationConfig scanConfig = new ScanOperationConfig()
                    {
                        Select = SelectValues.SpecificAttributes,
                        AttributesToGet = new List<string> { "TradeName" },
                        Filter = scanFilter
                    };
                    Search search = table.Scan(scanConfig);
                    List<Document> documentList = new List<Document>();
                    do
                    {
                        documentList=await search.GetNextSetAsync(default(CancellationToken));
                        foreach(var document in documentList)
                        {
                            var tradeName=document["TradeName"];
                            tradeNameList.Add(tradeName);
                        }
                    } while(!search.IsDone);
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug Trade Names From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug Trade Names From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug Trade Names From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return tradeNameList;
        }
    }
}
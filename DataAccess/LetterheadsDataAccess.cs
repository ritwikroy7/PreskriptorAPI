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
    public interface ILetterheadsDataAccess
    {
        Task<List<Letterhead>> GetAllLetterheadsAsync();
        Task SaveLetterheadAsync(Letterhead letterhead);
        Task<Letterhead> GetLetterheadAsync(string chamberName);
        Task DeleteLetterheadAsync(string chamberName);
        Task<List<string>> GetAllChamberNamesAsync();
    }
    
    public class LetterheadsDataAccess:ILetterheadsDataAccess
    {
        private readonly ILogger<LetterheadsDataAccess> _log;
        public LetterheadsDataAccess(ILogger<LetterheadsDataAccess> log)
        {
            _log=log;
        }
        public async Task<List<Letterhead>> GetAllLetterheadsAsync()
        {
            List<Letterhead> LetterheadList = new List<Letterhead>();
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient())
                {
                    var table = Table.LoadTable(dynamoClient,"HeaderMaster");
                    ScanFilter scanFilter = new ScanFilter();
                    Search search = table.Scan(scanFilter);
                    List<Document> documentList = new List<Document>();
                    do
                    {
                        documentList=await search.GetNextSetAsync(default(CancellationToken));
                        foreach(var document in documentList)
                        {
                            Letterhead letterhead = new Letterhead();
                            try
                            {
                                letterhead=JsonConvert.DeserializeObject<Letterhead>(document.ToJson());
                                LetterheadList.Add(letterhead);
                            }
                            catch(JsonException jEx)
                            {
                                _log.LogError("Json Deserialization Exception: "+jEx.Message);
                                throw new DataAccessException("An Error Occured While Retrieving Letterhead List From Database");
                            }
                        }
                    } while(!search.IsDone);
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Letterhead List From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Letterhead List From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Letterhead List From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return LetterheadList;
        }
        public async Task SaveLetterheadAsync(Letterhead letterhead)
        {
            var _letterheadJson = (string)null;
            try
            {
                _letterheadJson=JsonConvert.SerializeObject(letterhead);
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
                using (var dynamoClient = new AmazonDynamoDBClient())
                {
                    var table = Table.LoadTable(dynamoClient,"HeaderMaster");
                    var pItem = Document.FromJson(_letterheadJson);
                    document = await table.PutItemAsync(pItem,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Saving Letterhead To Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Saving Letterhead To Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Saving Letterhead To Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
        public async Task<Letterhead> GetLetterheadAsync(string chamberName)
        {
            var _letterhead = (Document)null;
            var _letterheadJson = (string)null;
            Letterhead letterhead = null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient())
                {
                    var table = Table.LoadTable(dynamoClient,"HeaderMaster");
                    _letterhead = await table.GetItemAsync(chamberName,default(CancellationToken));
                    if(_letterhead!=null)
                    {
                        _letterheadJson = _letterhead.ToJson();
                    }
                }
                if(_letterheadJson!=null)
                {
                    try
                    {
                        letterhead=JsonConvert.DeserializeObject<Letterhead>(_letterheadJson);
                    }
                    catch(JsonException jEx)
                    {
                        _log.LogError("Json Deserialization Exception: "+jEx.Message);
                        throw new DataAccessException("An Error Occured While Retrieving Letterhead From Database");
                    }
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Letterhead From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Letterhead From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Letterhead From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return letterhead;
        }
        public async Task DeleteLetterheadAsync(string chamberName)
        {
            Document document=null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient())
                {
                    var table = Table.LoadTable(dynamoClient,"HeaderMaster");
                    document = await table.DeleteItemAsync(chamberName,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Letterhead From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Letterhead From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Letterhead From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
        public async Task<List<string>> GetAllChamberNamesAsync()
        {
            List<string> chamberNameList = new List<string>();
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"HeaderMaster");
                    ScanFilter scanFilter = new ScanFilter();
                    ScanOperationConfig scanConfig = new ScanOperationConfig()
                    {
                        Select = SelectValues.SpecificAttributes,
                        AttributesToGet = new List<string> { "ChamberName" },
                        Filter = scanFilter
                    };
                    Search search = table.Scan(scanConfig);
                    List<Document> documentList = new List<Document>();
                    do
                    {
                        documentList=await search.GetNextSetAsync(default(CancellationToken));
                        foreach(var document in documentList)
                        {
                            var tradeName=document["ChamberName"];
                            chamberNameList.Add(tradeName);
                        }
                    } while(!search.IsDone);
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Chamber Names From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Chamber Names From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Chamber Names From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return chamberNameList;
        }
    }
}
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
    public interface ITestsDataAccess
    {
        Task<List<Test>> GetAllTestsAsync();
        Task SaveTestAsync(Test test);
        Task<Test> GetTestAsync(string type);
        Task DeleteTestAsync(string type);
        Task<List<string>> GetAllTypesAsync();
    }
    
    public class TestsDataAccess:ITestsDataAccess
    {
        private readonly ILogger<TestsDataAccess> _log;
        public TestsDataAccess(ILogger<TestsDataAccess> log)
        {
            _log=log;
        }
        public async Task<List<Test>> GetAllTestsAsync()
        {
            List<Test> TestList = new List<Test>();
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"TestMaster");
                    ScanFilter scanFilter = new ScanFilter();
                    Search search = table.Scan(scanFilter);
                    List<Document> documentList = new List<Document>();
                    do
                    {
                        documentList=await search.GetNextSetAsync(default(CancellationToken));
                        foreach(var document in documentList)
                        {
                            Test test = new Test();
                            try
                            {
                                test=JsonConvert.DeserializeObject<Test>(document.ToJson());
                                TestList.Add(test);
                            }
                            catch(JsonException jEx)
                            {
                                _log.LogError("Json Deserialization Exception: "+jEx.Message);
                                throw new DataAccessException("An Error Occured While Retrieving Test List From Database");
                            }
                        }
                    } while(!search.IsDone);
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test List From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test List From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test List From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return TestList;
        }
        public async Task SaveTestAsync(Test test)
        {
            var _testJson = (string)null;
            try
            {
                _testJson=JsonConvert.SerializeObject(test);
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
                    var table = Table.LoadTable(dynamoClient,"TestMaster");
                    var pItem = Document.FromJson(_testJson);
                    document = await table.PutItemAsync(pItem,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Saving Test To Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Saving Test To Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Saving Test To Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
        public async Task<Test> GetTestAsync(string type)
        {
            var _test = (Document)null;
            var _testJson = (string)null;
            Test test = null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"TestMaster");
                    _test = await table.GetItemAsync(type,default(CancellationToken));
                    if(_test!=null)
                    {
                        _testJson = _test.ToJson();
                    }
                }
                if(_testJson!=null)
                {
                    try
                    {
                        test=JsonConvert.DeserializeObject<Test>(_testJson);
                    }
                    catch(JsonException jEx)
                    {
                        _log.LogError("Json Deserialization Exception: "+jEx.Message);
                        throw new DataAccessException("An Error Occured While Retrieving Test From Database");
                    }
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return test;
        }
        public async Task DeleteTestAsync(string type)
        {
            Document document=null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"TestMaster");
                    document = await table.DeleteItemAsync(type,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Test From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Test From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Deleting Test From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
        public async Task<List<string>> GetAllTypesAsync()
        {
            List<string> typeList = new List<string>();
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
                {
                    var table = Table.LoadTable(dynamoClient,"TestMaster");
                    ScanFilter scanFilter = new ScanFilter();
                    ScanOperationConfig scanConfig = new ScanOperationConfig()
                    {
                        Select = SelectValues.SpecificAttributes,
                        AttributesToGet = new List<string> { "Type" },
                        Filter = scanFilter
                    };
                    Search search = table.Scan(scanConfig);
                    List<Document> documentList = new List<Document>();
                    do
                    {
                        documentList=await search.GetNextSetAsync(default(CancellationToken));
                        foreach(var document in documentList)
                        {
                            var type=document["Type"];
                            typeList.Add(type);
                        }
                    } while(!search.IsDone);
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test Types From Database"); 
            }
            catch (AmazonServiceException sEx)
            {
                _log.LogError("Amazon Service Exception: "+sEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test Types From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test Types From Database");
            }
            catch (Exception uEx)
            {
                _log.LogError("Unhandled Exception:  "+uEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return typeList;
        }
    }
}
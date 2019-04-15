using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace dmn_test
{
    internal class DmnClient
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string restApiEndpoint;

        public DmnClient(string restApiEndpoint)
        {
            this.restApiEndpoint = restApiEndpoint;
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            client.DefaultRequestHeaders.Add(
                "User-Agent", "WIGO4IT DmnClient");
        }

        public async Task<DmnDefinition> GetDefinition(string key){
            var stringTask = client.GetAsync($"{restApiEndpoint}/decision-definition/key/{key}/xml");
            var msg = await stringTask;
            var content = await msg.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DmnDefinition>(content);
        }

        public async Task<DmnResult> Evaluate(string key, DmnRequest request)
        {
            var stringTask = client.PostAsync($"{restApiEndpoint}/decision-definition/key/{key}/evaluate",
            new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            var msg = await stringTask;
            var content = await msg.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DmnResult>(content);
        }
    }

    public class DmnDefinition
    {
        [JsonProperty("id")]
        public string Id {get;set;}
        [JsonProperty("dmnXml")]
        public string DmnXml {get;set;}
    }

    public class DmnRequest
    {
        [JsonProperty("variables")]
        public Dictionary<string, Variable> Variables {get;set;}

        public DmnRequest(){
            Variables = new Dictionary<string, Variable>();
        }
    }

    public class Variable
    {
        [JsonProperty("value")]        
        public object Value {get;set;}
        [JsonProperty("type")]        
        public object Type {get;set;}

    }

    public class DmnResult : List<Dictionary<string,Variable>> 
    {
    }
}

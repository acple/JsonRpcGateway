using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonRpcGateway
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcRequest
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; }

        [JsonProperty("method")]
        public string Method { get; }

        [JsonProperty("params")]
        public JToken Parameters { get; }

        [JsonProperty("id")]
        public object Id { get; }

        public JsonRpcRequest(string method, JToken @params) : this(method, @params, null)
        { }

        public JsonRpcRequest(string method, JToken @params, object id) : this("2.0", method, @params, id)
        { }

        [JsonConstructor]
        public JsonRpcRequest(string jsonrpc, string method, JToken @params, object id)
        {
            this.JsonRpc = jsonrpc;
            this.Method = method;
            this.Parameters = @params;
            this.Id = id;
        }
    }
}

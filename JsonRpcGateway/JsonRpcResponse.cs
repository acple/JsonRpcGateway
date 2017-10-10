using Newtonsoft.Json;

namespace JsonRpcGateway
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcResponse
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc => "2.0";

        [JsonProperty("id")]
        public object Id { get; }

        protected JsonRpcResponse(object id)
        {
            this.Id = id;
        }

        public override string ToString()
            => JsonConvert.SerializeObject(this);

        public static implicit operator string(JsonRpcResponse response)
            => response.ToString();
    }
}

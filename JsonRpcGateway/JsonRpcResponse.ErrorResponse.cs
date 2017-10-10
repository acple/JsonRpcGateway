using Newtonsoft.Json;

namespace JsonRpcGateway
{
    public class ErrorResponse : JsonRpcResponse
    {
        [JsonProperty("error")]
        public JsonRpcException Error { get; }

        public ErrorResponse(object id, JsonRpcException exception) : base(id)
        {
            this.Error = exception;
        }
    }
}

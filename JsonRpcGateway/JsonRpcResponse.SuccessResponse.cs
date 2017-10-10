using Newtonsoft.Json;

namespace JsonRpcGateway
{
    public class SuccessResponse : JsonRpcResponse
    {
        [JsonProperty("result")]
        public object Result { get; }

        public SuccessResponse(object id, object result) : base(id)
        {
            this.Result = result;
        }
    }
}

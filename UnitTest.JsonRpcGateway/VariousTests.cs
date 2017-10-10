using ChainingAssertion;
using JsonRpcGateway;
using Xunit;

namespace UnitTest.JsonRpcGateway
{
    internal class VariousTestApi
    {
        public string JsonNullHandling(string a, string b, string c)
            => a + b + c;
    }

    public class VariousTests
    {
        private readonly JsonRpc<VariousTestApi> _jsonrpc = new JsonRpc<VariousTestApi>(new VariousTestApi());

        [Fact]
        public void JsonNullHandling()
        {
            var request = @"{""jsonrpc"":""2.0"",""method"":""JsonNullHandling"",""params"":[""abc"",null,""def""]}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<SuccessResponse>();

            response.Result.Is("abcdef");
        }

    }
}

namespace JsonRpcGateway
{
    public enum ErrorCode
    {
        ParseError = -32700, // parse error. not well formed
        InvalidRequest = -32600, // Invalid Request The JSON sent is not a valid Request object.
        MethodNotFound = -32601, // server error. requested method not found
        InvalidParameters = -32602, // server error. invalid method parameters
        InternalError = -32603, // server error. internal json-rpc error
        // [ -32000 .. -32099 ] server error. implementation-defined error
    }
}

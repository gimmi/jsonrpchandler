using Newtonsoft.Json;

namespace JsonRpcHandler
{
	public delegate void RpcHandlerInvoker(object instance = null, JsonSerializer jsonSerializer = null);
}
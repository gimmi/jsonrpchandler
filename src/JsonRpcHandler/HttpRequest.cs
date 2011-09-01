using System.IO;

namespace JsonRpcHandler
{
	public class HttpRequest
	{
		public string HttpMethod;
		public string ContentType;
		public TextReader Content;
	}
}
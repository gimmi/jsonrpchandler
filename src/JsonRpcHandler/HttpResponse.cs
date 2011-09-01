using System.IO;

namespace JsonRpcHandler
{
	public class HttpResponse
	{
		public string Status;
		public string ContentType;
		public TextWriter Content;
	}
}
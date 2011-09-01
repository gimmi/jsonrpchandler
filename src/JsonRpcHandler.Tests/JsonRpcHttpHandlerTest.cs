using System.IO;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Rhino.Mocks;
using SharpTestsEx;

namespace JsonRpcHandler.Tests
{
	[TestFixture]
	public class JsonRpcHttpHandlerTest
	{
		private JsonRpcHttpHandler _target;
		private static StringBuilder _responseContent;
		private static HttpResponse _httpResponse;
		private JsonRpcHandler _jsonRpcHandler;

		[SetUp]
		public void SetUp()
		{
			_jsonRpcHandler = MockRepository.GenerateStub<JsonRpcHandler>(null, null, null, null);
			_target = new JsonRpcHttpHandler();
			_responseContent = new StringBuilder();
			_httpResponse = new HttpResponse {
				Content = new StringWriter(_responseContent)
			};
		}

		[Test]
		public void Should_respond_ok_to_post_requests()
		{
			_jsonRpcHandler.Stub(x => x.Handle(JToken.Parse("\"request\""))).Return(JToken.Parse("\"response\""));
			_target.Handle(new HttpRequest {
				HttpMethod = "POST",
				ContentType = "application/json",
				Content = new StringReader("\"request\"")
			}, _httpResponse, _jsonRpcHandler);
			_httpResponse.Status.Should().Be.EqualTo("200 OK");
			_httpResponse.ContentType.Should().Be.EqualTo("application/json");
			_httpResponse.Content.ToString().Should().Be.EqualTo("\"response\"");
		}

		[Test]
		public void Should_respond_not_allowed_for_get_request()
		{
			_target.Handle(new HttpRequest { HttpMethod = "GET" }, _httpResponse, _jsonRpcHandler);
			_httpResponse.Status.Should().Be.EqualTo("405 Method Not Allowed");
		}

		[Test]
		public void Should_respond_not_allowed_for_put_request()
		{
			_target.Handle(new HttpRequest { HttpMethod = "PUT" }, _httpResponse, _jsonRpcHandler);
			_httpResponse.Status.Should().Be.EqualTo("405 Method Not Allowed");
		}

		[Test]
		public void Should_respond_unsupported_medie_for_non_json_request()
		{
			_target.Handle(new HttpRequest { HttpMethod = "POST", ContentType = "text/plain" }, _httpResponse, _jsonRpcHandler);
			_httpResponse.Status.Should().Be.EqualTo("415 Unsupported Media Type");
		}

		[Test]
		public void Should_respond_bad_request_when_cannot_parse_json()
		{
			_target.Handle(new HttpRequest {
				HttpMethod = "POST",
				ContentType = "application/json",
				Content = new StringReader("invalid json")
			}, _httpResponse, _jsonRpcHandler);
			_httpResponse.Status.Should().Be.EqualTo("400 Bad Request");
			_httpResponse.Content.ToString().Should().Contain("Newtonsoft.Json.JsonReaderException");
		}
	}
}
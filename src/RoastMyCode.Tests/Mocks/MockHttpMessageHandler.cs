using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RoastMyCode.Tests.Mocks
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseProducer;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseProducer)
        {
            _responseProducer = responseProducer;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseProducer(request));
        }
    }
}

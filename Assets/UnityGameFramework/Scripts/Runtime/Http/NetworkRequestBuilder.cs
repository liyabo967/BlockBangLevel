// NetworkRequestBuilder.cs
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class NetworkRequestBuilder
{
    private NetworkRequest request = new NetworkRequest();

    public NetworkRequestBuilder Url(string url)
    {
        request.url = url;
        return this;
    }

    public NetworkRequestBuilder Method(NetworkRequest.HttpMethod method)
    {
        request.method = method;
        return this;
    }

    public NetworkRequestBuilder Body(string body)
    {
        request.body = body;
        return this;
    }

    public NetworkRequestBuilder Body<T>(T data)
    {
        request.body = JsonConvert.SerializeObject(data);
        return this;
    }

    public NetworkRequestBuilder Header(string key, string value)
    {
        request.headers[key] = value;
        return this;
    }

    public NetworkRequestBuilder Headers(Dictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            request.headers[header.Key] = header.Value;
        }
        return this;
    }

    public NetworkRequestBuilder Timeout(int seconds)
    {
        request.timeout = seconds;
        return this;
    }

    public NetworkRequestBuilder Retry(int count)
    {
        request.retryCount = count;
        return this;
    }

    public NetworkRequestBuilder UseCache(bool use = true)
    {
        request.useCache = use;
        return this;
    }

    public NetworkRequestBuilder Tag(object tag)
    {
        request.tag = tag;
        return this;
    }

    public NetworkRequest Build()
    {
        return request;
    }
}
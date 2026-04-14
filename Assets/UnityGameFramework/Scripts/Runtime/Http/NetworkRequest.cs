// NetworkRequest.cs
using System;
using System.Collections.Generic;

[Serializable]
public class NetworkRequest
{
    public string url;
    public HttpMethod method = HttpMethod.GET;
    public Dictionary<string, string> headers = new Dictionary<string, string>();
    public byte[] bytes;
    public string body;
    public int timeout = 30;
    public int retryCount = 0;
    public bool useCache = false;
    public object tag; // 用于标识请求

    public NetworkRequest AddHeader(string key, string value)
    {
        if (headers == null) headers = new Dictionary<string, string>();
        headers[key] = value;
        return this;
    }

    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        DELETE,
        PATCH,
        HEAD,
        OPTIONS
    }
}

// INetworkClient.cs - 网络客户端接口
using System;
using System.Collections;

public interface INetworkClient
{
    void SendRequest(NetworkRequest request, Action<NetworkResponse> callback);
    IEnumerator SendRequestCoroutine(NetworkRequest request, Action<NetworkResponse> callback);
}

// IRequestInterceptor.cs - 请求拦截器接口
public interface IRequestInterceptor
{
    NetworkRequest OnRequest(NetworkRequest request);
}

// IResponseInterceptor.cs - 响应拦截器接口
public interface IResponseInterceptor
{
    NetworkResponse OnResponse(NetworkResponse response);
}

// IRequestSerializer.cs - 请求序列化接口
public interface IRequestSerializer
{
    string Serialize<T>(T obj);
    T Deserialize<T>(string json);
}

// IResponseDeserializer.cs - 响应反序列化接口
public interface IResponseDeserializer
{
    T Deserialize<T>(string data);
}
// NetworkResponse.cs
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class NetworkResponse
{
    public bool success;
    public int statusCode;
    public string error;
    public string errorMessage;
    
    public int code;
    public string data;
    
    public Dictionary<string, string> headers;
    public NetworkRequest originalRequest;
    public float elapsedTime;

    public NetworkResponse()
    {
        headers = new Dictionary<string, string>();
    }

    public T GetData<T>() where T : class
    {
        if (string.IsNullOrEmpty(data)) return null;
        try
        {
            var result = JsonConvert.DeserializeObject<T>(data);
            return result;
        }
        catch
        {
            return null;
        }
    }
}
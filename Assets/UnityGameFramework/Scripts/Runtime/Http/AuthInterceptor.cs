// AuthInterceptor.cs - 认证拦截器示例

using UnityEngine;

public class AuthInterceptor : IRequestInterceptor
{
    private string token;

    public AuthInterceptor(string token)
    {
        this.token = token;
    }

    public NetworkRequest OnRequest(NetworkRequest request)
    {
        if (!string.IsNullOrEmpty(token))
        {
            request.AddHeader("Authorization", $"Bearer {token}");
        }
        return request;
    }
}

// ErrorHandlerInterceptor.cs - 错误处理拦截器
public class ErrorHandlerInterceptor : IResponseInterceptor
{
    public NetworkResponse OnResponse(NetworkResponse response)
    {
        // if (!response.success)
        // {
        //     // 统一错误处理逻辑
        //     if (response.statusCode == 401)
        //     {
        //         // 处理未授权
        //         Debug.LogWarning("未授权，需要重新登录");
        //     }
        //     else if (response.statusCode >= 500)
        //     {
        //         // 处理服务器错误
        //         Debug.LogError("服务器错误");
        //     }
        // }
        return response;
    }
}
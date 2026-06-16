namespace FlowSpace.Infrastructure.Common;

public static class RedisConfiguration
{
    public static string NormalizeConnectionString(string redisUrl)
    {
        if (string.IsNullOrEmpty(redisUrl)) return string.Empty;
        
        if (!redisUrl.StartsWith("redis://") && !redisUrl.StartsWith("rediss://"))
        {
            return redisUrl;
        }

        try
        {
            var uri = new Uri(redisUrl);
            var host = uri.Host;
            var port = uri.Port == -1 ? (redisUrl.StartsWith("rediss://") ? 6380 : 6379) : uri.Port;
            var userInfo = uri.UserInfo.Split(':');
            var password = userInfo.Length > 1 ? userInfo[1] : userInfo[0];
            var ssl = redisUrl.StartsWith("rediss://") ? "ssl=true" : "ssl=false";

            // Upstash usually needs abortConnect=false
            return $"{host}:{port},password={password},{ssl},abortConnect=false";
        }
        catch
        {
            return redisUrl;
        }
    }
}

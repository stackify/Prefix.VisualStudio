using System;

namespace Prefix.VSExt2015
{
    public static class Helpers
    {
        public static string GetPrefixEndpoint()
        {
            var endpoint =
                Environment.GetEnvironmentVariable("PrefixServiceEndpoint", EnvironmentVariableTarget.Machine) ??
                "http://127.0.0.1:2012";

            if (endpoint.EndsWith("/") == false)
            {
                endpoint += "/";
            }
            return endpoint;

        }

        public static string GetPrefixVersion()
        {
            return Environment.GetEnvironmentVariable("PrefixServiceVersion", EnvironmentVariableTarget.Machine);
        }
    }
}

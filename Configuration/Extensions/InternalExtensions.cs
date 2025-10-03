namespace Configuration.Extensions
{
    public static class InternalExtensions
    {
        /// <summary>
        /// Function which handles the creation of the full path to be used in the request to the BE
        /// </summary>
        /// <param name="apiRequest">Request to be done to the BE</param>
        /// <param name="parameters">query string dictionary which passes the values inside to the BE (the key is the naming found in swagger, the value is the value we want to give it) </param>
        /// <param name="arguments">arguments that will be replaced in the route string (those found in the routes as {0},{1})</param>
        /// <returns></returns>
        public static string AsPath(this BackendRequests apiRequest, ICollection<KeyValuePair<string, string>> parameters = null, params object[] arguments)
        {
            var query = "?";
            if (parameters != null)
                query = parameters.Aggregate(query, (q, p) => $"{q}{p.Key}={p.Value}&");

            var routePath = RequestDictionary.ApiRequests[apiRequest];
            if (routePath == null)
                return null;

            if (arguments != null)
                routePath = string.Format(routePath, arguments);

            return routePath + query[..^1];
        }
    }
}

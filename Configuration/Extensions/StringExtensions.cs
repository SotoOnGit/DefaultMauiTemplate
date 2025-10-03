using Newtonsoft.Json;

namespace Configuration.Extensions
{
    public static class StringExtensions
    {
        public static TResponse DeSerialize<TResponse>(this string response) where TResponse : new()
        {
            try
            {
                if (typeof(TResponse) == typeof(List<string>))
                    response = response.Replace(@"\n", "").TrimStart('\"').TrimEnd('\"').Replace(@"\", "");

                return JsonConvert.DeserializeObject<TResponse>(response) ?? new TResponse();
            }
            catch (Exception)
            {
                return new TResponse();
            }
        }
    }
}

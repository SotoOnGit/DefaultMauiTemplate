using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Configuration.Extensions
{
    public static class ObjectExtensions
    {
        public static T CopyTo<T>(this object source) where T : new()
        {
            var result = new T();
            foreach (var pi in source.GetType().GetProperties())
            {
                var dstPi = result
                    .GetType()
                    .GetProperties()
                    .FirstOrDefault(p => p.Name == pi.Name && p.PropertyType == pi.PropertyType);
                if (dstPi == null || dstPi.GetSetMethod() == null) continue;
                dstPi.SetValue(result, pi.GetValue(source));
            }
            return result;
        }

        public static bool CopyTo<T>(this object source, T dst)
        {
            var modified = false;
            foreach (var pi in source.GetType().GetProperties())
            {
                var dstPi = dst
                    .GetType()
                    .GetProperties()
                    .FirstOrDefault(p => p.Name == pi.Name && p.PropertyType == pi.PropertyType && p.GetSetMethod() is not null);
                if (dstPi == null || dstPi.GetSetMethod() == null) continue;

                var dstValue = dstPi.GetValue(dst);
                var srcValue = pi.GetValue(source);
                if (dstValue == srcValue) continue;
                dstPi.SetValue(dst, srcValue);
                modified = true;
            }
            return modified;
        }

        public static StringContent ToJson(this object payload)
        {
            var body = new StringContent(JsonConvert.SerializeObject(payload));
            body.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return body;
        }
    }
}

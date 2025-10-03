using Common.Enums.Media;
using System.Net.Http.Headers;

namespace Configuration.Extensions
{
    public static class StreamExtensions
    {
        public static MultipartFormDataContent ToMultipart(this Stream payload, MediaType mediaType, string fileName = null)
        {
            var content = new MultipartFormDataContent();

            payload.Seek(0, SeekOrigin.Begin);
            var ss = new StreamContent(payload);

            if (fileName != null)
            {
                var extension = Path.GetExtension(fileName).Replace(".", "");

                var format = mediaType switch
                {
                    MediaType.Unknown => $"image/{extension}",
                    MediaType.Image => $"image/{extension}",
                    MediaType.Video => $"video/{extension}",
                    _ => $"image/{extension}",
                };

                ss.Headers.ContentType = MediaTypeHeaderValue.Parse(format);
            }
            else
                ss.Headers.ContentType = MediaTypeHeaderValue.Parse($"image/jpg");


            var fileNameToSave = fileName ?? "file.jpg";

            content.Add(ss, "file", fileNameToSave);

            return content;
        }

    }
}

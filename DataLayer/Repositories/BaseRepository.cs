using Configuration;
using Configuration.Extensions;
using Configuration.Models;
using DataLayer.Services;
using System.Net;

namespace DataLayer.Repositories
{
    public class BaseRepository
    {
        protected readonly NetworkService NetworkService;
        protected readonly IDefaultTemplateConfiguration Configuration;

        public BaseRepository(NetworkService networkService, IDefaultTemplateConfiguration configuration)
        {
            NetworkService = networkService;
            Configuration = configuration;
        }

        protected static Dictionary<string, string> BuildPaginationDictionary(int size, int page, string search, string orderBy, bool desc)
        {
            return new Dictionary<string, string>
            {
                { "size", size.ToString() },
                { "page", page.ToString() },
                { "search", search },
                { "orderby", orderBy},
                { "desc", desc.ToString() }
            };
        }

        protected async Task<CustomResponse<TEntity>> GetAsync<TEntity>(string url) where TEntity : new()
        {
            var response = await NetworkService.GetAsync(url).ConfigureAwait(false);
            return ProcessResponse<TEntity>(response);
        }

        protected async Task<CustomResponse<TEntity>> PostAsync<TEntity>(string url, object payload) where TEntity : new()
        {
            var response = await NetworkService.PostAsync(url, payload);
            return ProcessResponse<TEntity>(response);
        }

        protected async Task<CustomResponse> PostAsync(string url, object payload)
        {
            var response = await NetworkService.PostAsync(url, payload);
            return ProcessResponse(response);
        }

        protected async Task<CustomResponse<TEntity>> PutAsync<TEntity>(string url, object payload) where TEntity : new()
        {
            var response = await NetworkService.PutAsync(url, payload);
            return ProcessResponse<TEntity>(response);
        }

        protected async Task<CustomResponse> PutAsync(string url, object payload)
        {
            var response = await NetworkService.PutAsync(url, payload);
            return ProcessResponse(response);
        }

        protected async Task<CustomResponse<TEntity>> DeleteAsync<TEntity>(string url, object payload) where TEntity : new()
        {
            var response = await NetworkService.DeleteAsync(url, payload);
            return ProcessResponse<TEntity>(response);
        }

        protected async Task<CustomResponse> DeleteAsync(string url, object payload)
        {
            var response = await NetworkService.DeleteAsync(url, payload);
            return ProcessResponse(response);
        }


        public static CustomResponse<TEntity> ProcessResponse<TEntity>(ApiResponse response) where TEntity : new()
        {
            return response.StatusCode switch
            {
                HttpStatusCode.OK
                    => ProcessData<TEntity>(response),
                HttpStatusCode.BadRequest
                    => new CustomResponse<TEntity>
                    { ErrorMessage = GetError(response) },
                HttpStatusCode.RequestTimeout
                    => new CustomResponse<TEntity>()
                    {
                        Status = ConnectionStatusEnum.BadNetwork,
                        ErrorMessage = ErrorService.NoInternet
                    },
                HttpStatusCode.Unauthorized
                    => new CustomResponse<TEntity> { ErrorMessage = ErrorService.NeedLogOut },
                _
                    => new CustomResponse<TEntity>()
                    {
                        Status = ConnectionStatusEnum.OfflineServer,
                        ErrorMessage = ErrorService.ServerError
                    },
            };
        }

        private static CustomResponse ProcessResponse(ApiResponse response)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.OK
                    => new CustomResponse(),
                HttpStatusCode.BadRequest
                    => new CustomResponse
                    { ErrorMessage = GetError(response) },
                HttpStatusCode.RequestTimeout
                    => new CustomResponse()
                    {
                        Status = ConnectionStatusEnum.BadNetwork,
                        ErrorMessage = ErrorService.NoInternet
                    },
                HttpStatusCode.Unauthorized
                    => new CustomResponse { ErrorMessage = ErrorService.NeedLogOut },
                _
                    => new CustomResponse()
                    {
                        Status = ConnectionStatusEnum.OfflineServer,
                        ErrorMessage = ErrorService.ServerError
                    },
            };
        }

        private static CustomResponse<TEntity> ProcessData<TEntity>(ApiResponse response) where TEntity : new()
        {
            try
            {
                var data = response.Data.DeSerialize<TEntity>();
                return new CustomResponse<TEntity> { Entity = data };
            }
            catch (Exception e)
            {
                return new CustomResponse<TEntity> { ErrorMessage = e.Message };
            }
        }


        private static string GetError(ApiResponse response)
        {
            try
            {
                var errors = response.Data.DeSerialize<List<string>>().Aggregate((x, y) => x + ' ' + y);
                return errors;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Unknown Error.";
            }
        }
    }
}

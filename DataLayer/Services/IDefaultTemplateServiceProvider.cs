namespace DataLayer.Services
{
    public interface IDefaultTemplateServiceProvider
    {
        TService GetService<TService>();
    }
}

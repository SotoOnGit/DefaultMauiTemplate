namespace Configuration.Models
{
    public class CustomResponse<TEntity>
    {
        public TEntity Entity { get; set; }
        public string ErrorMessage { get; set; }
        public ConnectionStatusEnum Status { get; set; }
    }

    public class CustomResponse 
    {
        public string ErrorMessage { get; set; }
        public ConnectionStatusEnum Status { get; set; }
    }
}

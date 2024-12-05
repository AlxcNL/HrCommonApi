namespace HrCommonApi.Controllers.Responses
{
    public abstract class SimpleResponse : IResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

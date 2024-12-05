using HrCommonApi.Enums;

namespace HrCommonApi.Services.Base;

public class ServiceResult<TResult>(ServiceResponse response, TResult? entity = default, Exception? exception = null, string? message = null) where TResult : class
{
    public TResult? Result { get; } = entity;
    public ServiceResponse Response { get; } = response;
    public Exception? Exception { get; } = exception;
    public string? Message { get; } = message;
}

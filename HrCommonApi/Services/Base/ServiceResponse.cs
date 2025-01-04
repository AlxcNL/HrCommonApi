using HrCommonApi.Enums;

namespace HrCommonApi.Services.Base;

public class ServiceResponse<TResult>(ServiceCode code, TResult? entity = default, Exception? exception = null, string? message = null) where TResult : class
{
    public TResult? Result { get; } = entity;
    public ServiceCode Code { get; } = code;
    public Exception? Exception { get; } = exception;
    public string? Message { get; } = message;
}

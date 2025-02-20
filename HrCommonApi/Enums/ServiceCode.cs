﻿namespace HrCommonApi.Enums;

/// <summary>
/// The ServiceCode determines the kind of response you get from the API.
/// By sheer coincidence it also correlates to the <see cref="LogLevel">log level</see> for each response.
/// </summary>
public enum ServiceCode
{
    Success = 0,        // Log level: trace
    NotFound = 1,       // Log level: debug
    BadRequest = 2,     // Log level: information
    NotImplemented = 3, // Log level: warning
    Exception = 4       // Log level: error
}
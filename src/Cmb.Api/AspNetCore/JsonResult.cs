﻿using CSharpFunctionalExtensions;

namespace Cmb.Api.AspNetCore;

public record JsonResult<TResult, TError>
{
    public TResult Result { get; init; }
    public TError Error { get; init; }
    public bool IsSuccess { get; init; }

    public JsonResult()
    { }

    public static implicit operator JsonResult<TResult, TError>(Result<TResult, TError> r) => r.IsSuccess
        ? new() { Error = default, Result = r.Value, IsSuccess = true }
        : new() { Error = r.Error, Result = default, IsSuccess = false };

    public static implicit operator Result<TResult, TError>(JsonResult<TResult, TError> r) => r.IsSuccess
        ? r.Result
        : r.Error;
}


public record JsonOptionError
{
    public string Error { get; init; }
    public bool IsSuccess { get; init; }

    public JsonOptionError()
    { }

    public static implicit operator JsonOptionError(Result r) => r.IsSuccess
        ? new() { Error = default, IsSuccess = true }
        : new() { Error = r.Error, IsSuccess = false };
}
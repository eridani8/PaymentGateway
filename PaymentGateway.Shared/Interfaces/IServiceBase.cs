﻿using PaymentGateway.Shared.Types;

namespace PaymentGateway.Shared.Interfaces;

public interface IServiceBase
{
    Task<T?> PostRequest<T>(string url, object model);
    Task<T?> PostRequest<T>(string url);
    Task<Response> PostRequest(string url, object model);
    Task<Response> PostRequest(string url);
    Task<Response> GetRequest(string url);
    Task<Response> PutRequest(string url, object model);
    Task<Response> PutRequest(string url);
    Task<T?> PutRequest<T>(string url, object model);
    Task<T?> DeleteRequest<T>(string url);
}
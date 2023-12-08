using Amazon.Lambda.APIGatewayEvents;
using Devon4Net.Infrastructure.Common;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using System.Text;

namespace Devon4Net.Infrastructure.AWS.Lambda.Logs
{
    public static class ApiGatewayProxyLogs
    {
        private const string ResponseBodyEntity = "Response body";
        private const string RequestBodyEntity = "Request body";
        private const int MaxBodyResponse = 150;

        public static void LogsPostExecution(APIGatewayProxyRequest apiGatewayProxyRequest, APIGatewayProxyResponse apiGatewayProxyResponse)
        {
            try
            {
                if (Log.IsEnabled(LogEventLevel.Information))
                {
                    Devon4NetLogger.Information($"REQUEST {apiGatewayProxyRequest.RequestContext.RequestId} | HttpMethod: {apiGatewayProxyRequest.HttpMethod} | Path: {apiGatewayProxyRequest.Path} | Status Code: {apiGatewayProxyResponse.StatusCode}");
                    if (apiGatewayProxyResponse.StatusCode != 200) Devon4NetLogger.Debug(LogResponseBody(apiGatewayProxyRequest.RequestContext.RequestId, apiGatewayProxyRequest.Body, RequestBodyEntity));
                    Devon4NetLogger.Information(LogResponseBody(apiGatewayProxyRequest.RequestContext.RequestId, apiGatewayProxyResponse.Body, ResponseBodyEntity, true));
                }
                else if (Log.IsEnabled(LogEventLevel.Warning))
                {
                    if (apiGatewayProxyResponse.StatusCode < StatusCodes.Status200OK || apiGatewayProxyResponse.StatusCode >= StatusCodes.Status400BadRequest)
                    {
                        Devon4NetLogger.Warning($"REQUEST {apiGatewayProxyRequest.RequestContext.RequestId} | HttpMethod: {apiGatewayProxyRequest.HttpMethod} | Path: {apiGatewayProxyRequest.Path} | Status Code: {apiGatewayProxyResponse.StatusCode}");
                        Devon4NetLogger.Warning(LogResponseBody(apiGatewayProxyRequest.RequestContext.RequestId, apiGatewayProxyResponse.Body, ResponseBodyEntity, true));
                    }
                }
                else
                {
                    Devon4NetLogger.Debug($"REQUEST {apiGatewayProxyRequest.RequestContext.RequestId} | HttpMethod: {apiGatewayProxyRequest.HttpMethod} | Path: {apiGatewayProxyRequest.Path} | Status Code: {apiGatewayProxyResponse.StatusCode}");
                    Devon4NetLogger.Debug(LogResponseBody(apiGatewayProxyRequest.RequestContext.RequestId, apiGatewayProxyRequest.Body, RequestBodyEntity));
                    Devon4NetLogger.Debug(LogResponseBody(apiGatewayProxyRequest.RequestContext.RequestId, apiGatewayProxyResponse.Body, ResponseBodyEntity, true));
                }
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error($"REQUEST {apiGatewayProxyRequest.RequestContext.RequestId} | HttpMethod: {apiGatewayProxyRequest.HttpMethod} | Path: {apiGatewayProxyRequest.Path} | Status Code: {apiGatewayProxyResponse.StatusCode} | Exception: {ex.Message}");
                throw;
            }
        }

        private static string LogResponseBody(string identifier, string body, string entityDisclaimer, bool trimBody = false)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Request {identifier} {entityDisclaimer}: ");

            if (string.IsNullOrWhiteSpace(body))
            {
                stringBuilder.AppendLine("No data found.");
                return stringBuilder.ToString();
            }

            if (trimBody && body.Length > MaxBodyResponse)
            {
                stringBuilder.AppendLine(body[..MaxBodyResponse]);
            }
            else
            {
                stringBuilder.AppendLine(body);
            }

            return stringBuilder.ToString();
        }
    }
}

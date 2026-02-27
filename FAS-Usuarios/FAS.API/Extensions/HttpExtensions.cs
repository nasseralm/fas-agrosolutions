using FAS.API.Models;
using System.Text.Json;

namespace FAS.API.Extensions
{
    public static class HttpExtensions
    {
        //Extensão para colocar as informações no Header da aplicação, invocada pela Controller.
        public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
        {
            var jsonOptions = new JsonSerializerOptions {  PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            response.Headers["Pagination"] = JsonSerializer.Serialize(header, jsonOptions);
            response.Headers["Access-Control-Expose-Headers"] = "Pagination";

        }
    }
}
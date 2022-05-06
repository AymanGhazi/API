using System.Text.Json;
using API.Helpers;
using Microsoft.AspNetCore.Http;

namespace API.extensions
{
    public static class httpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, int currentpage, int itemsPerPage, int TotalItems, int totalPages)
        {
            var paginationHeader = new paginationHeader(currentpage, itemsPerPage, TotalItems, totalPages);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader, options));


            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}
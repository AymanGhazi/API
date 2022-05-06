using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Errors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.MiddelWare
{
    public class ExceptionMiddleWare
    {


        /// <summary>
        /// Here we have three injections RequestDelegate ,ILogger 
        /// ,IWebHostEnvironment
        /// 1- We must have Task InvokeAsync
        /// 2- HttpContext ==> RequestDelegate(HttpContext)
        /// 3- a- logging 
        ///    b- reponse set ==> 1- statuscode 2- ContentType
        ///    c- Response whether it is Development or Production
        ///    d- Json ==>OptionsSeralization json seralize for response
        ///    c- context write response json  
        /// ///  </summary>
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleWare> _logger;
        private readonly IHostEnvironment _env;
        public ExceptionMiddleWare(RequestDelegate next,
       ILogger<ExceptionMiddleWare> logger, IHostEnvironment env)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                //set response properities

                context.Request.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                // Response whether it is Development or Production
                var response = _env.IsDevelopment()
                ? new ApiException(context.Response.StatusCode,
                 ex.Message, ex.StackTrace?.ToString())
                : new ApiException(context.Response.StatusCode, "Internal Server Error ");


                //return Json
                var JsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(response, JsonOptions);

                await context.Response.WriteAsync(json);
            }

        }
    }
}
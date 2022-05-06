using System;
using System.Threading.Tasks;
using API.extensions;
using API.interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    //to update Last Active
    public class LogUserActivity : IAsyncActionFilter
    {
        //context of the Action Excuted before
        //context of the Action Excuted After
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();
            if (!resultContext.HttpContext.User.Identity.IsAuthenticated)
            {
                return;
            }
            //from calims principles
            var userId = resultContext.HttpContext.User.GetuserID();
            var UoW = resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();
            var user = await UoW.UserRepository.GetUserbyIdAsync(userId);
            user.LastActive = DateTime.UtcNow;
            await UoW.Complete();
            //go to Base Api Controler
        }
    }
}

//to filter 
//1- make the filter parameter properity
//2 - make the helper class to update the last Active
//3 - update Base Api Controller

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace ChargeBook.services {
    public interface IViewRenderService {
        Task<string> renderToStringAsync(string viewName, object model);
    }

    public class ViewRenderService : IViewRenderService {
        private readonly IRazorViewEngine razorViewEngine;
        private readonly ITempDataProvider tempDataProvider;
        private readonly IServiceProvider serviceProvider;

        public ViewRenderService(IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider) {
            this.razorViewEngine = razorViewEngine;
            this.tempDataProvider = tempDataProvider;
            this.serviceProvider = serviceProvider;
        }

        public async Task<string> renderToStringAsync(string viewName, object model) {
            var httpContext = new DefaultHttpContext {
                RequestServices = serviceProvider
            };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            await using (var stringWriter = new StringWriter()) {
                var viewResult = razorViewEngine.FindView(actionContext, viewName, false);
                if (viewResult.View == null) {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) {
                    Model = model
                };

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
                    stringWriter,
                    new HtmlHelperOptions()
                );
                await viewResult.View.RenderAsync(viewContext);
                return stringWriter.ToString();
            }
        }
    }
}
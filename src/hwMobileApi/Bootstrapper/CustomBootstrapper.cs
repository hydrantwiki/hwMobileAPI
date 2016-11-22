using System;
using System.Collections.Generic;
using System.Diagnostics;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Session;
using Nancy.TinyIoc;
using TreeGecko.Library.Common.Helpers;
using TreeGecko.Library.Loggly.TraceListeners;
using System.Linq;
using HydrantWiki.Library.Managers;

namespace HydrantWiki.Mobile.Api.Bootstrapper
{
    public class CustomBoostrapper : DefaultNancyBootstrapper
    {
        private string m_ApiToken;

        protected override void ConfigureRequestContainer(TinyIoCContainer _container, NancyContext _context)
        {
            base.ConfigureRequestContainer(_container, _context);
        }

        protected override void ApplicationStartup(TinyIoCContainer _container, IPipelines _pipelines)
        {
            base.ApplicationStartup(_container, _pipelines);
            m_ApiToken = Config.GetSettingValue("ApiAccessKey", null);

            Trace.Listeners.Add(new LogglyTraceListener());

            _pipelines.BeforeRequest.AddItemToStartOfPipeline(_context =>
            {
                //Validate the API Token
                string apiToken = _context.Request.Headers["ApiAccessKey"].FirstOrDefault();

                if (apiToken != null
                    && apiToken.Equals(m_ApiToken))
                {
                    return null;
                }

                TraceFileHelper.Warning("Missing or invalid api access key");
                return new Response()
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            });

            _pipelines.OnError.AddItemToStartOfPipeline((context, exception) =>
            {
                TraceFileHelper.Exception(exception);
                return Response.NoBody;
            });
        }


        
        protected override void ConfigureConventions(NancyConventions _conventions)
        {
            base.ConfigureConventions(_conventions);
           
            _conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("images", @"images")
            );

            _conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddFile("/api.html", "api.html")
            );
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(
                    _builder => _builder.StatusCodeHandlers = new List<Type>());
            }
        }
    }
}
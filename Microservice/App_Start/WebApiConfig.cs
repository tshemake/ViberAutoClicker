using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microservice.Models;
using Unity;
using Unity.Lifetime;

namespace Microservice
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Конфигурация и службы веб-API
            UnityContainer container = new UnityContainer();
            container.RegisterType<IStatusRepository, StatusRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<IMessageRepository, MessageRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<IHistoryRepository, HistoryRepository>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);

            // Маршруты веб-API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}

using Microsoft.Xrm.Sdk;
using System;

namespace Plugins.Warehouse
{
    public interface ILocalPluginContext
    {
        IPluginExecutionContext PluginExecutionContext { get; }
        IOrganizationServiceFactory OrgSvcFactory { get; }
        ITracingService TracingService { get; }
    }

    internal sealed class LocalPluginContext : ILocalPluginContext
    {
        public LocalPluginContext(IServiceProvider serviceProvider)
        {
            TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            OrgSvcFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        }

        public IPluginExecutionContext PluginExecutionContext { get; private set; }
        public IOrganizationServiceFactory OrgSvcFactory { get; private set; }
        public ITracingService TracingService { get; private set; }
    }

    public abstract class PluginBase : IPlugin
    {
        private readonly Type _pluginType;

        protected PluginBase(Type pluginType)
        {
            _pluginType = pluginType;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            var localPluginContext = new LocalPluginContext(serviceProvider);
            try
            {
                ExecuteDataversePlugin(localPluginContext);
            }
            catch (Exception exception)
            {
                localPluginContext.TracingService.Trace("Plugin {0} failed: {1}", _pluginType.FullName, exception);
                throw;
            }
        }

        protected abstract void ExecuteDataversePlugin(ILocalPluginContext localPluginContext);
    }
}

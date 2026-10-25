using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TALXIS.DevKit.Build.Dataverse.Plugin;

namespace Plugins.Warehouse
{
    [PluginStep("Create", "lab_warehousetransaction", PluginStage.PostOperation)]
    public class SubtractQuantityPlugin : PluginBase
    {
        public SubtractQuantityPlugin() : base(typeof(SubtractQuantityPlugin))
        {
        }

        public SubtractQuantityPlugin(string unsecureConfiguration, string secureConfiguration)
            : this()
        {
        }

        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            var context = localPluginContext.PluginExecutionContext;
            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity target) ||
                target.LogicalName != "lab_warehousetransaction")
            {
                return;
            }

            if (!target.Contains("lab_quantity") || !target.Contains("lab_itemid") || !target.Contains("lab_transactiontype"))
            {
                return;
            }

            if (!(target["lab_quantity"] is int quantity) || !(target["lab_itemid"] is EntityReference itemReference) ||
                !(target["lab_transactiontype"] is OptionSetValue transactionType))
            {
                return;
            }

            var availableService = localPluginContext.OrgSvcFactory.CreateOrganizationService(context.UserId);
            var item = availableService.Retrieve("lab_warehouseitem", itemReference.Id, new ColumnSet("lab_availablequantity"));
            var available = item.GetAttributeValue<int>("lab_availablequantity");

            if (transactionType.Value == 100000000)
            {
                available += quantity;
            }
            else if (transactionType.Value == 100000001)
            {
                available -= quantity;
            }
            else
            {
                return;
            }

            var updatedItem = new Entity("lab_warehouseitem", itemReference.Id)
            {
                ["lab_availablequantity"] = available
            };
            availableService.Update(updatedItem);
        }
    }
}
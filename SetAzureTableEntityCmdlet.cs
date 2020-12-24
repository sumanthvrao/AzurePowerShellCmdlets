using System;
using System.Management.Automation;
using Microsoft.Azure.Cosmos.Table;

namespace AzurePowerShellCmdlets
{
    [Cmdlet(VerbsCommon.Set, "AzureTableEntity")]
    public class SetAzureTableEntityCmdlet : Cmdlet
    {
        [Parameter(Position = 0, HelpMessage = "The Azure Table URI (Including the Name)", Mandatory = true)]
        public string tableUri;

        [Parameter(Position = 1, HelpMessage = "Storage Account Name", Mandatory = true)]
        public string storageAccountName;

        [Parameter(Position = 2, HelpMessage = "Storage Account Key", Mandatory = true)]
        public string storageAccountKey;

        [Parameter(Position = 3, HelpMessage = "RowKey of the target row", Mandatory = true)]
        public string rowKey;

        [Parameter(Position = 4, HelpMessage = "PartitionKey of the target row", Mandatory = true)]
        public string partitionKey;

        [Parameter(Position = 5, HelpMessage = "Entity key to replace", Mandatory = true)]
        public string entityKey;

        [Parameter(Position = 6, HelpMessage = "Entity value to replace with", Mandatory = true)]
        public string entityValue;

        public bool success;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var cloudTable = new CloudTable(new Uri(tableUri), new StorageCredentials(storageAccountName, storageAccountKey));
            success = _UpdateEntity(cloudTable, partitionKey, rowKey, entityKey, entityValue);
            WriteObject(this.success);
        }

        private static bool _UpdateEntity(CloudTable table, string partitionKey, string rowKey, string entityKey, string entityValue)
        {
            // Reference : https://microsoft.github.io/AzureTipsAndTricks/blog/tip85.html
            //             https://github.com/uglide/azure-content/blob/master/articles/storage/storage-dotnet-how-to-use-tables.md
            //             https://stackoverflow.com/questions/61564135/is-there-any-method-to-update-single-row-single-property-in-azure-table
            try
            {
                DynamicTableEntity updateEntity = new DynamicTableEntity(partitionKey, rowKey);
                updateEntity.Properties.Add(entityKey, new EntityProperty(entityValue));
                TableOperation mergeOperation = TableOperation.InsertOrMerge(updateEntity);
                table.Execute(mergeOperation);
                Console.WriteLine($"{DateTime.Now} :: Azure Table entity updated {partitionKey}/{rowKey}/{entityKey} => {entityValue}");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
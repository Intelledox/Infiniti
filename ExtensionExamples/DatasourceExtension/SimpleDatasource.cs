using Intelledox.Extension.Datasource;
using Intelledox.Model;
using Intelledox.QAWizard;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SampleDatasourceExtensions
{
    /// <summary>
    /// This sample demonstrates a basic 'Hello World' level data source extension
    /// </summary>
    public class SimpleDatasource : DatasourceConnector
    {
        private Guid _tableObjectType = new Guid("CFE1653E-E2F7-4713-BFBE-311E8BD484D3");
        private Guid _serviceObjectType = new Guid("498B7D35-5D05-4728-B07A-DD5C7B7DF083");

        // The identity is used for registering the action within Infiniti. The id and the name needs
        // to be unique and the name is displayed to the user in Manage.
        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity()
            {
                Id = new Guid("7D647B92-78E3-4EBC-923F-1A95C15C4C2C"),
                Name = "Infiniti Simple Data Source Extension"
            };

        public override Task<DataTable> GetAvailableFilterFieldsAsync(string connectionString, Query query, Authentication auth, DatasourceProperties properties)
        {
            // Return all the things that designers can use to limit the result in a call to GetDataAsync. For data bases this is typically column names, for 
            // web APIs - parameter names, etc.
            // Forms the basis of what can be passed in the 'criteria' parameter when getting data.

            var filterFields = new DataTable();
            filterFields.Columns.Add("Filter1", typeof(int));
            filterFields.Columns.Add("Filter2", typeof(string));

            return Task.FromResult(filterFields);
        }

        public override Task<DataTable> GetDataAsync(string connectionString, Query query, DataFilter criteria, Authentication auth, DatasourceProperties properties)
        {
            // Main function of a data source that is called whenever a form needs data from the external service.
            // Connect to the service at 'connectionString', accessing the resource in 'query', matching the filters in 'criteria', using the credentials in 'auth'

            var data = new DataTable();
            data.Columns.Add("Column1", typeof(int));
            data.Columns.Add("Column2", typeof(string));

            data.BeginLoadData();
            DataRow currentRow = data.NewRow();
            currentRow["Column1"] = 123;
            currentRow["Column2"] = "ABC";
            data.Rows.Add(currentRow);
            data.EndLoadData();

            return Task.FromResult(data);
        }

        public async override Task<IDataReader> GetDataReaderAsync(string connectionString, Query query, DataFilter criteria, Authentication auth, DatasourceProperties properties)
        {
            // This GetData call is used when repeating documents are being created. To support a large number of rows a streaming DataReader is requested.
            // If the service can't support that, create a DataReader wrapper from a DataTable.

            return (await GetDataAsync(connectionString, query, criteria, auth, properties)).CreateDataReader();
        }

        public override Task<DataTable> GetSchemaAsync(string connectionString, Query query, DataFilter criteria, Authentication auth, DatasourceProperties properties)
        {
            // Returns all the available columns/fields of the service at 'connectionString', for the resource in 'query'

            var data = new DataTable();
            data.Columns.Add("Column1", typeof(int));
            data.Columns.Add("Column2", typeof(string));

            return Task.FromResult(data);
        }

        public override Task<string[]> ObjectListAsync(string connectionString, string prefixText, Guid objectType, Authentication auth, DatasourceProperties properties)
        {
            // Return all the data objects in the service at 'connectionString', accessing the resource in 'query'
            // Filter the results to those whose names begin with 'prefixText'

            string[] availableObjects = { "SampleTable1", "SampleTable2" };

            return Task.FromResult(availableObjects.Where(s => s.StartsWith(prefixText, StringComparison.OrdinalIgnoreCase)).ToArray());
        }

        public override ObjectType[] ObjectTypes()
        {
            // Different types of objects that this data source can connect to. This may trigger the data source to behave differently based on the
            // value that gets passed in 'query.ObjectType' when getting data or schemas.

            return new ObjectType[] {
                new ObjectType(_tableObjectType, "Table", multiline: false, canCache: true, selectableSchemaFields: false),
                new ObjectType(_serviceObjectType, "Service", multiline: false, canCache: true, selectableSchemaFields: false)
            };
        }

        public override Task<bool> TestConnectionAsync(string connectionString, Authentication auth, DatasourceProperties properties)
        {
            // Test that a connection to the service in 'connectionString' can be made using the credentials in 'auth';

            return Task.FromResult(true);
        }
    }
}

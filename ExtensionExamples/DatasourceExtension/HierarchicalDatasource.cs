using Intelledox.Extension.Datasource;
using Intelledox.Model;
using Intelledox.QAWizard;
using System;
using System.Data;
using System.Threading.Tasks;

namespace SampleDatasourceExtensions
{
    /// <summary>
    /// This sample demonstrates an advanced data source extension that supports hierarchical/nested data
    /// </summary>
    public class HierarchicalDatasource : DatasourceConnector
    {
        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity()
            {
                Id = new Guid("C657EB87-D974-496D-BC8C-EA046FA60F58"),
                Name = "Infiniti Hierarchical Data Source Extension"
            };

        public override Task<DataTable> GetSchemaAsync(string connectionString, Query query, DataFilter criteria, Authentication auth, DatasourceProperties properties)
        {
            // Schema requires all of the available fields be returned in a flattened structure. All levels are added
            // as columns to the same DataTable. Nested field names must start with the same prefix as their parent column.

            var data = new DataTable();
            data.Columns.Add("Column1", typeof(int));
            data.Columns.Add("ColumnNested", typeof(DataTable));    // A nested parent column is identified as being of type DataTable
            data.Columns.Add("ColumnNested.ChildColumn", typeof(string));

            return Task.FromResult(data);
        }

        public override Task<DataTable> GetDataAsync(string connectionString, Query query, DataFilter criteria, Authentication auth, DatasourceProperties properties)
        {
            // Data at the root level is constructed as normal. Only the fields relevant at this level are added.
            var data = new DataTable();
            data.Columns.Add("Column1", typeof(int));
            data.Columns.Add("ColumnNested", typeof(DataTable));

            data.BeginLoadData();
            for (int i = 1; i < 3; i++)
            {
                DataRow currentRow = data.NewRow();
                currentRow["Column1"] = i;


                // Nested data is constructed as it's own DataTable and then inserted as the value of the
                // cell in the parent row. This is typically implemented as recursive function calls to handle 
                // data at any level.
                var nestedData = new DataTable();
                nestedData.Columns.Add("ColumnNested.ChildColumn", typeof(string));

                DataRow currentNestedRow = nestedData.NewRow();
                currentNestedRow["ColumnNested.ChildColumn"] = "SubValue" + i.ToString();
                nestedData.Rows.Add(currentNestedRow);


                currentRow["ColumnNested"] = nestedData;
                data.Rows.Add(currentRow);
            }
            data.EndLoadData();

            return Task.FromResult(data);
        }

        public override Task<DataTable> GetAvailableFilterFieldsAsync(string connectionString, Query query, Authentication auth, DatasourceProperties properties)
        {
            return Task.FromResult(new DataTable());
        }

        public async override Task<IDataReader> GetDataReaderAsync(string connectionString, Query query, DataFilter criteria, Authentication auth, DatasourceProperties properties)
        {
            return (await GetDataAsync(connectionString, query, criteria, auth, properties)).CreateDataReader();
        }

        public override Task<string[]> ObjectListAsync(string connectionString, string prefixText, Guid objectType, Authentication auth, DatasourceProperties properties)
        {
            return Task.FromResult(new string[0]);
        }

        public override ObjectType[] ObjectTypes()
        {
            return new ObjectType[] {
                new ObjectType(new Guid("223731A5-0AA9-439F-9024-B17268DD0CE3"), "Service", false, true, false)
            };
        }

        public override Task<bool> TestConnectionAsync(string connectionString, Authentication auth, DatasourceProperties properties)
        {
            return Task.FromResult(true);
        }
    }
}

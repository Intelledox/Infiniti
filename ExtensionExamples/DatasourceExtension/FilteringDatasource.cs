using Intelledox.Extension.Datasource;
using Intelledox.Model;
using Intelledox.QAWizard;
using System;
using System.Data;
using System.Threading.Tasks;

namespace SampleDatasourceExtensions
{
    /// <summary>
    /// This sample demonstrates recursively iterating through data filters and determining what comparison to make.
    /// The filtering of data sources is largely implementation specific so this sample code has the basic recursive structure
    /// that needs to be filled in.
    /// To limit the type of comparisons that are supported override the SupportedFilterComparisons function. For data sources
    /// that just pass parameters to an external service, just return the Intelledox.Model.ComparisonType.Equals enum.
    /// </summary>
    public class FilteringDatasource : DatasourceConnector
    {
        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity()
            {
                Id = new Guid("D4205921-330C-483F-BCFB-B6424AD2FC9B"),
                Name = "Infiniti Filtering Data Source Extension"
            };

        public override Task<DataTable> GetAvailableFilterFieldsAsync(string connectionString, Query query, Authentication auth, DatasourceProperties properties)
        {
            var filterFields = new DataTable();
            filterFields.Columns.Add("Column1", typeof(string));

            return Task.FromResult(filterFields);
        }

        public override Task<DataTable> GetDataAsync(string connectionString, Query query, DataFilter criteria, Authentication auth, DatasourceProperties properties)
        {
            var data = new DataTable();
            data.Columns.Add("Column1", typeof(string));

            data.BeginLoadData();
            DataRow currentRow = data.NewRow();
            currentRow["Column1"] = "ABC";
            data.Rows.Add(currentRow);
            data.EndLoadData();

            if (criteria != null)
            {
                BuildFilteringStatementRecursive(criteria);

                // Implementation Detail: Filter data with the filtering statement build from BuildFilteringStatementRecursive
            }

            return Task.FromResult(data);
        }

        public async override Task<IDataReader> GetDataReaderAsync(string connectionString, Query query, DataFilter criteria, Authentication auth, DatasourceProperties properties)
        {
            return (await GetDataAsync(connectionString, query, criteria, auth, properties)).CreateDataReader();
        }

        public override Task<DataTable> GetSchemaAsync(string connectionString, Query query, DataFilter criteria, Authentication auth, DatasourceProperties properties)
        {
            var data = new DataTable();
            data.Columns.Add("Column1", typeof(string));

            return Task.FromResult(data);
        }

        public override Task<string[]> ObjectListAsync(string connectionString, string prefixText, Guid objectType, Authentication auth, DatasourceProperties properties)
        {
            return Task.FromResult(new string[0]);
        }

        public override ObjectType[] ObjectTypes()
        {
            return new ObjectType[] {
                new ObjectType(new Guid("04E62357-696D-4165-AD5C-8B1A5EE6CAAF"), "Table", multiline: false, canCache: true, selectableSchemaFields: false)
            };
        }

        public override Task<bool> TestConnectionAsync(string connectionString, Authentication auth, DatasourceProperties properties)
        {
            return Task.FromResult(true);
        }

        protected void BuildFilteringStatementRecursive(DataFilter filter)
        {
            try
            {
                if (filter.DataFilterType == ClauseType.AllOf || filter.DataFilterType == ClauseType.AnyOf)
                {
                    // Within a grouping filter. Keep recursively building our statement with child filters.

                    foreach (DataFilter subFilter in filter.SubFilters)
                    {
                        BuildFilteringStatementRecursive(subFilter);

                        if (filter.DataFilterType == ClauseType.AllOf)
                        {
                            // Wrap in an 'AND" statement with the next filter
                        }
                        else
                        {
                            // Wrap in an 'OR' statement with the next filter
                        }
                    }
                }
                else if (filter.DataFilterType == ClauseType.Comparison)
                {
                    // Filter the data field filter.FieldName using filter.FieldValue

                    if (filter.FieldValue != null)
                    {
                        GetComparisonTypeSignVal(filter);
                    }
                }

            }
            catch (Exception ex)
            {
                LogEvent(ex.ToString(), EventLevel.Errored);
            }
        }

        protected void GetComparisonTypeSignVal(DataFilter filter)
        {
            switch (filter.CompareType)
            {
                case ComparisonType.BeginsWith:
                    // filter.FieldName like filter.FieldValue + *
                    break;

                case ComparisonType.Contains:
                    // filter.FieldName like * + filter.FieldValue + *
                    break;

                case ComparisonType.NotContain:
                    // filter.FieldName not like * + filter.FieldValue + *
                    break;

                case ComparisonType.EndsWith:
                    // filter.FieldName like * + filter.FieldValue
                    break;

                case ComparisonType.GreaterThan:
                    // filter.FieldName > filter.FieldValue
                    break;

                case ComparisonType.GreaterThanOrEquals:
                    // filter.FieldName >= filter.FieldValue
                    break;

                case ComparisonType.LessThan:
                    // filter.FieldName < filter.FieldValue
                    break;

                case ComparisonType.LessThanOrEquals:
                    // filter.FieldName <= filter.FieldValue
                    break;

                case ComparisonType.NotEqualTo:
                    // filter.FieldName <> filter.FieldValue
                    break;

                case ComparisonType.ContainedIn:
                    // filter.FieldValue like * + filter.FieldName + *
                    break;

                case ComparisonType.NotContainedIn:
                    // filter.FieldValue not like * + filter.FieldName + *
                    break;

                default:
                    // filter.FieldName == filter.FieldValue
                    break;
            }
        }
    }
}

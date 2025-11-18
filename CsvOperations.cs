using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using ProjectLaborBackend.Controllers;
using ProjectLaborBackend.Entities;
using System.ComponentModel;
using System.Data;
using ProjectLaborBackend.Services;
using System.IO;
namespace ProjectLaborBackend
{
    public class CsvOperations
    {
        public CsvOperations()
        {
        }

        public byte[] ExportDataToExcel(AppDbContext.Tables table)
        {
            // Connection string to the MySQL database
            DotNetEnv.Env.Load();
            string connectionString = DotNetEnv.Env.GetString("DB_CONNECTION_STRING");

            // SQL query to fetch data
            string query = $"SELECT * FROM {table.ToString()}";

            // Create a connection to the MySQL database
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();

                // Fill the DataTable with the result set
                adapter.Fill(dataTable);

                // Specify license type as NonCommercial
                ExcelPackage.License.SetNonCommercialPersonal("ProjectLabor");

                // Create a new Excel package to work with Excel files
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    // Add a new worksheet to the Excel package
                    var worksheet = excelPackage.Workbook.Worksheets.Add(table.ToString());

                    // Load the DataTable into the Excel worksheet
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                    // Save the Excel file
                    return excelPackage.GetAsByteArray();
                }
            }
        }

        public List<List<string>> ImportDataFromExcel(IFormFile file)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ProjectLabor");

            List<List<string>> data = new List<List<string>>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];

                        int colCount = worksheet.Dimension.End.Column;
                        int rowCount = worksheet.Dimension.End.Row;

                        HashSet<int> idColumns = new HashSet<int>();
                        for (int col = 1; col <= colCount; col++)
                        {
                            string header = worksheet.Cells[1, col].Text?.Trim();
                            if (string.Equals(header, "Id", StringComparison.OrdinalIgnoreCase))
                            {
                                idColumns.Add(col);
                            }
                        }

                        for (int rowIdx = 2; rowIdx <= rowCount; rowIdx++)
                        {
                            List<string> row = new List<string>();

                            for (int col = 1; col <= colCount; col++)
                            {
                                if (idColumns.Contains(col))
                                    continue;

                                string value = worksheet.Cells[rowIdx, col].Text?.Trim();
                                row.Add(value ?? "");
                            }

                            data.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading Excel file.", ex);
            }

            return data;
        }

        public byte[] GenerateTemplateForTable(AppDbContext.Tables table)
        {
            DotNetEnv.Env.Load();
            string connectionString = DotNetEnv.Env.GetString("DB_CONNECTION_STRING");

            string query = $"SELECT * FROM {table.ToString()} WHERE 1=0";

            DataTable dataTable = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dataTable);
                }
            }

            ExcelPackage.License.SetNonCommercialPersonal("ProjectLabor");

            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add(table.ToString());

                int colIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (string.Equals(column.ColumnName, "Id", StringComparison.OrdinalIgnoreCase))
                        continue;

                    worksheet.Cells[1, colIndex].Value = column.ColumnName;

                    worksheet.Cells[2, colIndex].Value = $"Enter {column.ColumnName}";

                    colIndex++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return excelPackage.GetAsByteArray();
            }
        }



    }
}

using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace Devon4Net.Infrastructure.Common.IO
{
    public static class CsvOperations
    {
        public static List<T> GetCsvData<T>(StreamReader reader, CsvConfiguration configuration = default)
        {
            if (configuration == null)
            {
                configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    LeaveOpen = true,
                    Mode = CsvMode.NoEscape,
                    MissingFieldFound = null,
                    PrepareHeaderForMatch = x => x.Header.ToLower()
                };
            }

            using var csv = new CsvReader(reader, configuration);
            return csv.GetRecords<T>().ToList();
        }

        public static Stream CreateCsv<T>(List<T> items, CsvConfiguration configuration = default)
        {
            if (configuration == null)
            {
                configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    LeaveOpen = true,
                    Mode = CsvMode.NoEscape
                };
            }

            var mem = new MemoryStream();
            using var writer = new StreamWriter(mem, encoding: Encoding.GetEncoding("iso-8859-1"), leaveOpen: true);
            using var csvWriter = new CsvWriter(writer, configuration);
            csvWriter.WriteRecords(items);

            writer.Flush();

            mem.Position = 0;
            return mem;
        }
    }
}

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace Sessionizing.Domain
{
    
    public class CSVReader : IReader
    {
        private string directory;
        private List<string> files = new List<string>();
        
        public CSVReader(string path)
        {
            this.directory = path;
        }
        
        public List<IEnumerable<PageView>> ReadData()
        {
            files = this.getCSVFileNames();
            var allfiles = new List<IEnumerable<PageView>>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            foreach (var currentCSVName in this.files)
            {
                using (var reader = new StreamReader(currentCSVName))
                using (var csv = new CsvReader(reader, config))
                {
                    var file = csv.GetRecords<PageView>();
                    allfiles.Add(file);

                }
                
            }

            return allfiles;

        }

        private List<string> getCSVFileNames()
        {
           return Directory.GetFiles(directory, "*.csv").ToList();
        }
    }
}
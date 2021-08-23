using System;
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
        private int totalRecords { get; set; }

        public CSVReader(string path)
        {
            this.directory = path;
        }
        
        public List<IEnumerator<PageView>> ReadData()
        {
            this.totalRecords = 0;
            files = this.getCSVFileNames();
            List<IEnumerator<PageView>> allPageViewsListsIterators = new List<IEnumerator<PageView>>();
            
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            
            foreach (var currentCSVFileName in this.files)
            {
                var reader = new StreamReader(currentCSVFileName);
                var csv = new CsvReader(reader, config);
                
                IEnumerable<PageView> pageViewsListIterator = csv.GetRecords<PageView>();
                // this.totalRecords += pageViewsListIterator.Count();
                    // foreach (PageView currentPageView in pageViewsListIterator)
                    // {
                    //     Console.WriteLine($"url: {currentPageView.url}");
                    // }
                allPageViewsListsIterators.Add(pageViewsListIterator.GetEnumerator());
            }

            return allPageViewsListsIterators;
        }

        private List<string> getCSVFileNames()
        {
           return Directory.GetFiles(directory, "*.csv").ToList();
        }
    }
}
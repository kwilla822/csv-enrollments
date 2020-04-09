using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace Enrollments
{
    public class EnrollmentCsvParser
    { 
        public static void SplitFile(string csvFilename)
        {
            try
            {
                var latestEnrollments = ReadLatestVersion(csvFilename);
                SaveByGroup(latestEnrollments);
            }
            catch 
            {
                throw;
            }
        }

        private static IEnumerable<IGrouping<string, Enrollment>> ReadLatestVersion(string filename)
        {
            var enrollments = new List<Enrollment>();

            using (var reader = new StreamReader(filename))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // CsvReader does yield results for GetRecords, but LINQ operations
                // on the entire result set could have negative impact on performance
                // for parsing larger files.

                //Iterating over the records seems more stable here. 
                while (csv.Read())
                {
                    var enrollment = csv.GetRecord<Enrollment>();

                    var existingEnrollment = enrollments.FirstOrDefault(
                            x => x.UserId.Equals(enrollment.UserId, StringComparison.InvariantCultureIgnoreCase) &&
                            x.InsuranceCompany.Equals(enrollment.InsuranceCompany, StringComparison.InvariantCultureIgnoreCase)
                        );
                    
                    if (existingEnrollment == null)
                    {
                        enrollments.Add(enrollment);
                    }
                    else if (enrollment.Version > existingEnrollment.Version)
                    { 
                        enrollments.Remove(existingEnrollment);
                        enrollments.Add(enrollment);
                    }                    
                }
            }

            var ordering = enrollments.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);
            var grouping = ordering.GroupBy(x => x.InsuranceCompany);

            return grouping;
        }

        private static void SaveByGroup(IEnumerable<IGrouping<string, Enrollment>> enrollments)
        {
            foreach (var group in enrollments)
            {
                //Assumes all company names are valid file names.
                using (var writer = new StreamWriter($"{group.Key}.csv"))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<Enrollment>();
                    group.ToList().ForEach(enrollment =>
                    {
                        csv.NextRecord();
                        csv.WriteRecord(enrollment);
                    });
                }
            }
        }
    }
}

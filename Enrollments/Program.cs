using System;

namespace Enrollments
{
    class Program
    {
        static void Main(string[] args)
        {
            const string csvFilename = "Enrollments.csv";

            try
            {
                EnrollmentCsvParser.SplitFile(csvFilename);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while trying to split file.  Filename: {csvFilename}, Error: {ex}");
            }
        }
    }
}

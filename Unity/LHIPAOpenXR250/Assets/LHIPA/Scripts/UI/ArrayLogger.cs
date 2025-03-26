using System;
using System.IO;

namespace lhipa
{
    public static class ArrayLogger
    {
        /// <summary>
        /// Logs a float array to a file in CSV-like format.
        /// Each float is separated by a semicolon, and a period is used as the decimal separator.
        /// If the file does not exist, it will be created.
        /// </summary>
        /// <param name="array">The array of floats to log.</param>
        /// <param name="filename">The file to which the array should be logged.</param>
        public static void LogArrayToFile(float[] array, string filename)
        {
            // Convert the float array to a single semicolon-separated string
            string formattedLine = string.Join(";",
                Array.ConvertAll(array, x => x.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)));

            // Ensure the directory for the file exists
            string directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Append the formatted line to the file
            using StreamWriter writer = new StreamWriter(filename, append: true);
            writer.WriteLine(formattedLine);
        }
    }
}
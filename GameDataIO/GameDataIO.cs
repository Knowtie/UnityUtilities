using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Assets.Data.GameData {

    /// <summary>
    /// This class reads and writes [Serializable] data from a file, and provides a utility function
    /// to convert tab delimted text data to be persisted into structures using reflection.
    /// </summary>
    public class GameDataIO {

        /// <summary>
        /// Persists an entire list into a the named file.
        /// </summary>
        /// <typeparam name="T">A struct or class that has been marked with [Serializable].</typeparam>
        /// <param name="aData">List of T.</param>
        /// <param name="aFilename">Filename to overwrite.</param>
        public static void Persist<T>(List<T> aData, string aFilename) {
            if ( ! IsSerializable<T>())
                throw new ArgumentException("T is not [Serializable]");

            using (var FILE = File.OpenWrite(aFilename)) {
                new BinaryFormatter().Serialize(FILE, aData); // Writes the entire list.
            }
        }

        /// <summary>
        /// Same as Persist except data is persisted to Assets/AppData and constructs 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aData"></param>
        /// <param name="aFilename"></param>
        public static void PersistAppData<T>(List<T> aData, string aFilename) {
            string DIRECTORY = Application.dataPath.Replace("/", "\\") + @"\AppData\";
            if (! Directory.Exists(DIRECTORY))
                Directory.CreateDirectory(DIRECTORY);
            Persist<T>(aData, DIRECTORY + aFilename);
        }

        /// <summary>
        /// Loads a list of data from a file.
        /// </summary>
        /// <typeparam name="T">A struct or class that has been marked with [Serializable].</typeparam>
        /// <param name="aFilename">Name of the file to load (must exist).</param>
        /// <returns>A List of T.</returns>
        public static List<T> Load<T>(string aFilename) {
            if ( ! IsSerializable<T>())
                throw new ArgumentException("T is not [Serializable]");

            using (var FILE = File.OpenRead(aFilename)) {
                return (List<T>) new BinaryFormatter().Deserialize(FILE); // Reads the entire list.
            }
        }
        
        /// <summary>
        /// Same as load, except data is loaded from Assets/AppData
        /// </summary>
        /// <typeparam name="T">A struct or class that has been marked with [Serializable].</typeparam>
        /// <param name="aFilename">Name of the file to load (must exist).</param>
        /// <returns>A List of T.</returns>
        public static List<T> LoadAppData<T>(string aFilename) {
            string DIRECTORY = Application.dataPath.Replace("/", "\\") + @"\AppData\";
            if (!Directory.Exists(DIRECTORY)) {
                throw new ArgumentException("Assets/AppData doesn't exist");
            }
            if (!File.Exists(DIRECTORY+aFilename))
                throw new ArgumentException(aFilename + " doesn't exist in Assets/AppData");

            return Load<T>(Application.dataPath.Replace("/", "\\") + @"\AppData\" + aFilename);
        }

        /// <summary>
        /// Tests to see if the type provided is serializable
        /// </summary>
        /// <typeparam name="T">Type to test</typeparam>
        /// <returns>Returns true.</returns>
        private static bool IsSerializable<T>() {
            return typeof(T).IsDefined(typeof(SerializableAttribute),true);
        }

        /// <summary>
        /// Converts multi-line tab delimited string data into a list of type T.  Used primarily to 
        /// initialize struct data to be persisted using the Persist method that came from a spreadsheet.  Note, this method
        /// does not attempt to validate the data, if it cannot converted, T.column returns the default value.
        /// </summary>
        /// <typeparam name="T">Type of data that will be persisted.</typeparam>
        /// <param name="aText">The text you want to convert.  If it's in the clipboard, use EditorGUIUtility.systemCopyBuffer.</param>
        /// <param name="aConverters">Is a dictionary of methods that converts string data to the corresponding type.</param>
        /// <returns>List of T.</returns>
        public static List<T> ConvertText<T>( string aText , Dictionary<Type,Func<String,object>> aConverters) where T : new() {

            // Get all the properties of the data we want to convert into
            PropertyInfo[] INFOS = typeof(T).GetProperties();

            // Create a new list
            List<T> DATA = new List<T>();

            using (StringReader READER = new StringReader(aText)) {
                string LINE;

                // Read a new row of data
                while ((LINE = READER.ReadLine()) != null) {
                    T ROW = new T();
                    List<string> COLUMNS = LINE.Split("\t".ToCharArray()).ToList();

                    // Go through the columns, only convert what is available (extra text columns are ignored, extra T attributes are uninitialized
                    for (int INDEX = 0; INDEX < Math.Min(COLUMNS.Count, INFOS.Length); INDEX++) {

                        PropertyInfo INFO = INFOS[INDEX];

                        // Do we have a converter for the T.column's type?
                        if (aConverters.ContainsKey(INFO.PropertyType)) {
                            try {
                                // Convert the string data
                                object RESULT = aConverters[INFO.PropertyType](COLUMNS[INDEX]);

                                // Save the converted data into the row
                                INFO.SetValue(ROW,RESULT,null);
                            } catch {
                                // ignore - could not be converted
                            }
                        } // else ignore - could not be converted
                    }

                    DATA.Add(ROW);
                }
            }

            return DATA;
            
        }

        /// <summary>
        /// Gets a standard set of converters to be used with ConvertText.  After getting back the dictionary,
        /// you can manipulate the converters, or add your own using:
        /// <code>
        ///   var DEF = GetDefaultConverters;
        ///   DEF[typeof (bool)] = (STRING => STRING.ToUpper() == "YES");       // new bool converter
        ///   DEF.Add(typeof(TimeSpan),STRING => ... );     // Add a converter for a time span
        ///   // etc.
        /// </code>
        /// </summary>
        /// <returns>A set of data converters you use to in the Conver</returns>
        public static Dictionary<Type, Func<String, object>> GetDefaultConverters() {
            Dictionary<Type,Func<String,object>> CONVERTERS = new Dictionary<Type, Func<string, object>>();
            CONVERTERS.Add(typeof(string),STRING => STRING);

            CONVERTERS.Add(typeof(short),STRING => Convert.ToInt16(STRING));
            CONVERTERS.Add(typeof(int),STRING => Convert.ToInt32(STRING));
            CONVERTERS.Add(typeof(long),STRING => Convert.ToInt64(STRING));

            CONVERTERS.Add(typeof(float), STRING => Convert.ToSingle(STRING));
            CONVERTERS.Add(typeof(decimal), STRING => Convert.ToDecimal(STRING));
            CONVERTERS.Add(typeof(double), STRING => Convert.ToDouble(STRING));

            // YYYYMMDD
            CONVERTERS.Add(typeof (DateTime), STRING => new DateTime(Convert.ToInt32(STRING.Substring(0, 4)), Convert.ToInt32(STRING.Substring(4, 2)), Convert.ToInt32(STRING.Substring(6, 2))));

            // TRUE/FALSE
            CONVERTERS.Add(typeof(bool),STRING => STRING.ToUpper() == "TRUE");

            return CONVERTERS;
        }

    }
}

# GameDataIO - Utility Class - Overview

The purpose of this utility class is to be able to initialize, write and later read static data stored in data files into a Unity game/app or C# application (.Net or Mono).  It also contains a method for converting tab delimited data into object instances, and this is highly convenient for converting text-based spreadsheet data to objects and be persisted.

Data gets persisted in a binary format; which is very data efficient for storage and reading data. While no game data is impervious to tampering, data in a binary format is harder to manipulate by the casual gamer.

Data persistance is very simple.  It is provided in a List<> and serialized to a file.  A file is desearlized into a List<>.

# Persisting Data

Before you can save/load data, you must construct a class.  Only the properties of the class will be persisted and loaded.  The system provides no method to update your data if you decide to change your class definition later on.

The system provides two ways to initialize your data:

1. Construct one or more object and store them in a List<T> then use GameDataIO.Persist() to save them.

2. Construct a text format with rows and columns separated by TAB characters, and convert the string data to the format of your object type.  Data can be converted from any text source.  The example below uses EditorGUIUtility.systemCopyBuffer which is the Unity wrapper for the Windows or Mac clipboard.

For "2", start by creating your data you want to persist.  The most obvious choice for this is to use a spreadsheet program such as Microsoft Excel or Open Office, which can copy the data to the clipboard in the above format.

How **GameDataIO.ConvertText<>(...)** decides to transform your data is done by using Reflection to extract the properties of your class and a set of String to Object Converters.  These can be obtained via **GameDataIO.GetDefaultConverters()**, which you can then modify to support any kind of data format (see below):

The example below shows how you could copy your data from the spreadsheet program into use your system's clipboard (Mac/Windows).  You simply attach this script to an object, copy your data into the clipboard then start your unity app.  This will load the data and save it into a file.  *The order of the columns in your clipboard data must match the order of the properties in your class with not gaps.*

    public class DataLoaderDummy : MonoBehaviour {

        public void Start() {
            List<GameElement> ELEMENTS = GameDataIO.ConvertText<GameElement>(
                                              EditorGUIUtility.systemCopyBuffer, 
                                              GameDataIO.GetDefaultConverters());   
                                              
            GameDataIO.Persist(ELEMENTS,@"Z:\GameElements.dat");
        }
    }

"**EditorGUIUtility.systemCopyBuffer**" is the system's clipboard (Windows or Mac)

"**GameDataIO.GetDefaultConverters()**" is a dictionary of data converters that can transform string data into the required items in the type *GameElement*.  

"**GameDataIO.Persist(List,Filename)**" persists a List<T> structure to the named file.

# Persisting AppData

The class also has a PersistAppData<T>(...) method for saving data to Assets/AppData; this way data can be included in a distribution and always be found in an installation without additional distribution work.  This has the same parameters as Persist<T>() except that there is no path information in the filename.

# Retreiving Data

To get the same data back:

    List<GameElement> ELEMENTS = GameDataIO.Load<GameElement>(@"Z:\GameElements.dat");
    
# Retreving AppData

To get the same data back that has been stored in Assets/AppData.  See Persisting AppData for more info:

    List<GameElement> ELEMENTS = GameDataIO.LoadAppData<GameElement>("GameElements.dat");
    
# Converters

Converters are used by **GameDataIO.ConvertText<>** to transform string data into an object of a specific type.

You can create your own dictionary of converters or modify or add to the default ones.  For every property in your object needs to have all of it's data in a single tab delimited value in your conversion file.  For example, suppose you want to convert Vector3 data.  Inside the cell of your spreadsheet, you'd specify 3 numbers delimited by commas ... ==>1,2,3<== The default converters don't have a Vector3 converter, but this is how you could do it.

    var CONVERTERS = GameDataIO.GetDefaultConverters();
    CONVERTERS.Add(typeof(Vector3),delegate( string aString ) {
      try {
          string[] PARTS = aString.Split(",".ToCharArray());
          float X = Convert.ToSingle(PARTS[0]);
          float Y = Convert.ToSingle(PARTS[1]);
          float Z = Convert.ToSingle(PARTS[2]);

          return new Vector3(X, Y, Z);
      } catch {
        return new Vector3(0,0,0);
      }
    });

**Note** An alternate method to storing all your vector data in a single cell is to create a class that mimimcs the columns of your spreadsheet.  After you load the data from the spreadsheet, you convert that data into the format you want to persist.  **OR**, you can construct objects directly then persist the list to a file.

If you need to replace a converter, such as the one for DateTime or bool values, you can say:

    var CONVERTERS = GameDataIO.GetDefaultConverters();
    
    CONVERTERS[typeof(bool)] = ( STRING => STRING.ToUpper() == "ON" );
    
# GameDataIO is not a database

**Important Note** While you could use this system as a basic database, it doesn't have a mechanism for reading/writing individual data rows, so loading means reading the entire file and saving means writing out the entire list each time.  It's really designed for generally static data that is represented by lists and needs to be loaded entirely by your game/application.


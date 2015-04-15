# Game Data IO

The purpose of this utility is to be able to initialize, then write and later read static data stored in data files
into a Unity game.

You first start by creating your data you want to persist in a tab delimited format with new rows separated by new lines.  The most obvious choice for this is to use a spreadsheet program such as Microsoft Excel or Open Office.

The example below shows how you could copy your data from the spreadsheet program into use your system's clipboard (Mac/Windows).  You simply attach this script to an object, copy your data into the clipboard then start your unity app.  This will load the data and save it into a file.

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

To get the same data back:

    List<GameElement> ELEMENTS = GameDataIO.Load<GameElement>(@"Z:\GameElements.dat");
    
You can create your own dictionary, or modify the default one.  But for every property in your object needs to have all of it's data in a single tab delimited value in your conversion file.  For example, suppose you want to convert Vector3 data.  Inside the cell of your spreadsheet, you'd specify 3 numbers delimited by commas ... ==>1,2,3<== The default converters don't have a Vector3 converter, but this is how you could do it.

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

If you need to replace a converter, such as the one for DateTime or bool values, you can say:

    var CONVERTERS = GameDataIO.GetDefaultConverters();
    CONVERTERS[typeof(bool)] = ( STRING => STRING.ToUpper() == "ON" );
    

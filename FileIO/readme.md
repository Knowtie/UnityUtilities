FileIO is a set of C# utility classes for persisting data into files.  Unlike GameDataIO which is limited to a serialized data of a single type, FileIO doesn't work with serializes.  It's intent is to create an uncoupled data storage mechanism that can handle any type you want.  It's format is extremely efficient as stores binary data in the files with no markup (unless you want it).

When this library was created, it was designed to store vertex, triangle and uv information for 4 Unity3D Meshes and restore them quickly.  The example below is how to implement one mesh.  The difference between 4 meshes and 1 is nothing more than a loop to store or retrieve 4 one after the other.

The the library comes with two different types of persistence mechanisms.  One for persisting List<T> and another for persisting a single object instance.  These are referred to as Filers; so there is a ListFiler for List<T> and a DataFiler for a single instance.  It's relatively easy to create others, and how to do this is described below.

All a Filer does is store or retrieve data according to two methods you include in your constructor.  For example:

    private readonly ListFiler<Vector3> _Vector3 = 
	        new ListFiler<Vector3>( FileUtility.WriteVector3 , 
			                        FileUtility.ReadVector3);

This Filer can persist and load List<Vector3> data.  It works by writing a header, following a list length, followed by sets of 3 floats for each entry in the list.  All this information is stored as binary.  To read the same data back, you read the header, then based on the header call this Filer to pull back the data into a list.

How it decides to write your data is based on two methods you include in the constructor.  In the example above, FileUtility.WriteVector3 and FileUtility.ReadVector3, which looks like this:

    public static void WriteVector3(FileStreamIO aFilestream, Vector3 aVector3) {
        aFilestream.Write(aVector3.x,BitConverter.GetBytes);
        aFilestream.Write(aVector3.y,BitConverter.GetBytes);
        aFilestream.Write(aVector3.z,BitConverter.GetBytes);
    }
	
    public static Vector3 ReadVector3(FileStreamIO aFileStream) {
        float X = ReadFloat(aFileStream);
        float Y = ReadFloat(aFileStream);
        float Z = ReadFloat(aFileStream);
        return new Vector3(X,Y,Z);
    }

FileStreamIO is another utility class that is included and provides some basic decorations to the FileStream which handles all the actual data reading/writing.

The constructor for ListFiler:

    public ListFiler(Persister<T> aPersister, Loader<T> aLoader) {
        _Persister = aPersister;
        _Loader = aLoader;
    }
	
Persister and Loader are two delegates that look like this:

    public delegate void Persister<in T>(FileStreamIO IO, T aData);
    public delegate T Loader<out T>(FileStreamIO IO);
	
How to persist a mesh to a file?

    public class FileMesh {

    private const int cEndOfMesh = 0;
    private const int cEndOfData = -1;
    private const int cVertices = 1;
    private const int cTriangles = 2;
    private const int cUV = 3;

    readonly ListFiler<Vector3> _Vector3 = new ListFiler<Vector3>( FileUtility.WriteVector3 , FileUtility.ReadVector3);
    readonly ListFiler<Vector2> _Vector3 = new ListFiler<Vector2>( FileUtility.WriteVector2 , FileUtility.ReadVector2);
    readonly ListFiler<int> _Int = new ListFiler<int>( FileUtility.WriteInt , FileUtility.ReadInt);

    /// <summary>
    /// Method for persisting a mesh in a binary format to a file.  Note: Does not close FileStreamIO, you have 
    /// to call FileStreamIO.Close() yourself.
    /// </summary>
    /// <param name="aMesh"></param>
    /// <param name="aIndex"></param>
    /// <param name="IO"></param>
    public void PersistMesh(Mesh aMesh, int aIndex, FileStreamIO IO) {

        _Vector3.Persist(cVertices,aMesh.vertices.ToList(),IO);
        _Int.Persist(cTriangles,aMesh.triangles.ToList(),IO);
        _Vector2.Persist(cUV,aMesh.uv.ToList(),IO);
            
        IO.Write(cEndOfMesh, BitConverter.GetBytes);

    }

    public void LoadMesh(Mesh aMesh, FileStreamIO aIO) {
        aMesh.Clear();

        int TYPE = FileUtility.ReadInt(aIO);

        while (TYPE != cEndOfMesh) {
            switch (TYPE) {
                case cVertices:
                    aMesh.vertices = _Vector3.Load(aIO).ToArray();
                    break;
                case cTriangles:
                    aMesh.triangles = _Integer.Load(aIO).ToArray();
                    break;
                case cUV:
                    aMesh.uv = _Vector2.Load(aIO).ToArray();
                    break;
            }

            TYPE = FileUtility.ReadInt(aIO);
                
        }

    }

    }

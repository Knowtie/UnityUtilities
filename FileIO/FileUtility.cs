using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Code.Utility.FileIO {

    /// <summary>
    /// Provides support utilities reading and loading different types of structured data.
    /// 
    /// Extend as necessary.
    /// </summary>
    public class FileUtility {

        public static int ReadInt(FileStreamIO aFileStream) {
            byte[] BYTES = aFileStream.Read(4);
            return BitConverter.ToInt32(BYTES, 0);
        }

        public static float ReadFloat(FileStreamIO aFileStream) {
            byte[] BYTES = aFileStream.Read(4);
            return BitConverter.ToSingle(BYTES, 0);
        }

        public static Vector2 ReadVector2(FileStreamIO aFileStream) {
            float X = ReadFloat(aFileStream);
            float Y = ReadFloat(aFileStream);
            return new Vector2(X,Y);
        }

        public static Vector3 ReadVector3(FileStreamIO aFileStream) {
            float X = ReadFloat(aFileStream);
            float Y = ReadFloat(aFileStream);
            float Z = ReadFloat(aFileStream);
            return new Vector3(X,Y,Z);
        }

        public static void WriteVector3(FileStreamIO aFilestream, Vector3 aVector3) {
            aFilestream.Write(aVector3.x,BitConverter.GetBytes);
            aFilestream.Write(aVector3.y,BitConverter.GetBytes);
            aFilestream.Write(aVector3.z,BitConverter.GetBytes);
        }
        public static void WriteVector2(FileStreamIO aFilestream, Vector2 aVector2) {
            aFilestream.Write(aVector2.x,BitConverter.GetBytes);
            aFilestream.Write(aVector2.y,BitConverter.GetBytes);
        }
        public static void WriteInt(FileStreamIO aFilestream, int aInt) {
            aFilestream.Write(aInt,BitConverter.GetBytes);
        }

    }
}

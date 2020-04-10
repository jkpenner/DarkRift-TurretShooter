using DarkRift;
using UnityEngine;

public static class DarkRiftExtensions {
    public static void Write(this DarkRiftWriter writer, Vector3 vec) {
        writer.Write(vec.x);
        writer.Write(vec.y);
        writer.Write(vec.z);
    }

    public static Vector3 ReadVector3(this DarkRiftReader reader) {
        return new Vector3(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );
    }
}
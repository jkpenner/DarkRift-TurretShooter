using DarkRift;
using UnityEngine;

public class ProjectileData : IDarkRiftSerializable {
    public Vector3 Position { get; set; }
    public float Rotation { get; set; }

    public void Deserialize(DeserializeEvent e) {
        this.Position = e.Reader.ReadVector3();
        this.Rotation = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e) {
        e.Writer.Write(this.Position);
        e.Writer.Write(this.Rotation);
    }
}

public class ProjectileSpawnData : IDarkRiftSerializable {
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    public void Deserialize(DeserializeEvent e) {
        this.Position = e.Reader.ReadVector3();
        this.Rotation = e.Reader.ReadVector3();
    }

    public void Serialize(SerializeEvent e) {
        e.Writer.Write(this.Position);
        e.Writer.Write(this.Rotation);
    }
}
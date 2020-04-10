using DarkRift;
using UnityEngine;

public class TurretSpawnData : IDarkRiftSerializable {
    public ushort ID { get; set; }
    public ushort Index { get; set; }

    public void Deserialize(DeserializeEvent e) {
        this.ID = e.Reader.ReadUInt16();
        this.Index = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e) {
        e.Writer.Write(this.ID);
        e.Writer.Write(this.Index);
    }
}

public class TurretUpdateData : IDarkRiftSerializable {
    public ushort ClientId { get; set; }
    public float Rotation { get; set; }

    public void Deserialize(DeserializeEvent e) {
        this.ClientId = e.Reader.ReadUInt16();
        this.Rotation = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e) {
        e.Writer.Write(this.ClientId);
        e.Writer.Write(this.Rotation);
    }
}

public class TurretFireData : IDarkRiftSerializable {
    public ushort Barrel { get; set; }
    public float Rotation { get; set; }

    public void Deserialize(DeserializeEvent e) {
        this.Barrel = e.Reader.ReadUInt16();
        this.Rotation = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e) {
        e.Writer.Write(this.Barrel);
        e.Writer.Write(this.Rotation);
    }
}
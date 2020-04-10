using DarkRift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyData : IDarkRiftSerializable {
    public uint ID { get; set; }
    public Vector3 Position { get; set; }

    public void Deserialize(DeserializeEvent e) {
        this.ID = e.Reader.ReadUInt32();
        this.Position = e.Reader.ReadVector3();
    }

    public void Serialize(SerializeEvent e) {
        e.Writer.Write(this.ID);
        e.Writer.Write(this.Position);
    }
}

public class EnemyDespawnData : IDarkRiftSerializable {
    public uint ID { get; set; }

    public void Deserialize(DeserializeEvent e) {
        this.ID = e.Reader.ReadUInt32();
    }

    public void Serialize(SerializeEvent e) {
        e.Writer.Write(this.ID);
    }
}

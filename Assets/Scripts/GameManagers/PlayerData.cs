using DarkRift;

public class PlayerData : IDarkRiftSerializable {
    public ushort ID { get; set; }
    public int Number { get; set; }
    public bool IsActive { get; set; }

    public void Deserialize(DeserializeEvent e) {
        this.ID = e.Reader.ReadUInt16();
        this.Number = e.Reader.ReadInt32();
        this.IsActive = e.Reader.ReadBoolean();
    }

    public void Serialize(SerializeEvent e) {
        e.Writer.Write(this.ID);
        e.Writer.Write(this.Number);
        e.Writer.Write(this.IsActive);
    }

    public bool Set(PlayerData data) {
        bool hasChanged = false;

        if (this.ID != data.ID) {
            this.ID = data.ID;
            hasChanged = true;
        }

        if (this.Number != data.Number) {
            this.Number = data.Number;
            hasChanged = true;
        }

        if (this.IsActive != data.IsActive) {
            this.IsActive = data.IsActive;
            hasChanged = true;
        }

        return hasChanged;
    }
}

public class ServerPlayerData : PlayerData {
    public bool IsReady { get; private set; }

    public ServerPlayerData() {
        this.IsReady = false;
    }

    public void MarkIsReady() {
        this.IsReady = true;
    }
}
namespace DiscordBot.Models
{
    public class SerializedDiscordUser : IEquatable<SerializedDiscordUser>
    {
        public ulong Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string GlobaName { get; set; } = string.Empty;
        public int RaidsCount { get; set; }

        public SerializedDiscordUser(ulong id, string name, int raidsCount)
        {
            Id = id;
            Name = name;
            RaidsCount = raidsCount;
        }

        public bool Equals(SerializedDiscordUser? other)
        {
            if (other == null) return false;

            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(other.Name))
            {
                return false;
            }

            if (Name != other.Name)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            return Equals(obj as SerializedDiscordUser);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }
}

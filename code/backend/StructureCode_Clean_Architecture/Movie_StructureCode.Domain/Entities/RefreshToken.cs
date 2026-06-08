using System;

namespace Movie_StructureCode.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Jti { get; private set; } = string.Empty;
        public string AccessJti { get; private set; } = string.Empty;
        public Guid UserId { get; private set; }
        public AppUser? AppUser { get; private set; }

        public bool IsRevoked { get; private set; } = false;
        public bool IsUsed { get; private set; } = false;
        
        public DateTime? UsedAt { get; private set; }
        public DateTime ExpiredAt { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public string? DeviceName { get; private set; }
        public string? DeviceType { get; private set; }
        public string? Ip { get; private set; }
        public DateTime? LoginAt { get; private set; }

        private RefreshToken() { }

        private RefreshToken(
            Guid id,
            string jti,
            string accessJti,
            Guid userId,
            bool isRevoked,
            bool isUsed,
            DateTime? usedAt,
            DateTime expiredAt,
            DateTime createdAt,
            string? deviceName,
            string? deviceType,
            string? ip,
            DateTime? loginAt)
        {
            Id = id;
            Jti = jti;
            AccessJti = accessJti;
            UserId = userId;
            IsRevoked = isRevoked;
            IsUsed = isUsed;
            UsedAt = usedAt;
            ExpiredAt = expiredAt;
            CreatedAt = createdAt;
            DeviceName = deviceName;
            DeviceType = deviceType;
            Ip = ip;
            LoginAt = loginAt;
        }

        public static RefreshToken Create(
            Guid id,
            string jti,
            string accessJti,
            DateTime expiredAt,
            Guid userId,
            string? deviceName,
            string? deviceType,
            string? ip,
            DateTime? loginAt)
        {
            return new RefreshToken(
                id,
                jti,
                accessJti,
                userId,
                false,
                false,
                null,
                expiredAt,
                DateTime.UtcNow,
                deviceName,
                deviceType,
                ip,
                loginAt ?? DateTime.UtcNow);
        }

        public void Revoke()
        {
            IsRevoked = true;
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
        }

        public void RevokeAndMarkUsed()
        {
            IsRevoked = true;
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
        }
    }
}

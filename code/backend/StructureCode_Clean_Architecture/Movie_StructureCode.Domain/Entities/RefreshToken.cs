namespace Movie_StructureCode.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Token { get; private set; }
        public string JwtId { get; private set; }

        public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;
        public DateTime? LastUsedDate { get; private set; }
        public DateTime ExpriseDate { get; private set; }

        public bool IsRevoked { get; private set; } = false;
        public bool IsUsed { get; private set; } = false;
        // forein key
        public Guid UserId { get; private set; }
        public AppUser? AppUser { get; private set; }

        private RefreshToken() { }

        private RefreshToken(
            Guid id,
            string token,
            string jwtId,
            DateTime createdDate,
            DateTime? lastUsedDate,
            DateTime expriseDate,
            bool isRevoked,
            bool isUsed,
            Guid userId)
        {
            Id = id;
            Token = token;
            JwtId = jwtId;
            CreatedDate = createdDate;
            LastUsedDate = lastUsedDate;
            ExpriseDate = expriseDate;
            IsRevoked = isRevoked;
            IsUsed = isUsed;
            UserId = userId;
        }

        public static RefreshToken Create(
            Guid id,
            string token,
            string jwtId,
            DateTime expriseDate,
            Guid userId)
        {
            return new RefreshToken(
                id,
                token,
                jwtId,
                DateTime.UtcNow,
                null,
                expriseDate,
                false,
                false,
                userId);
        }

        public void Revoke()
        {
            IsRevoked = true;
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
            LastUsedDate = DateTime.UtcNow;
        }

        public void UpdateLastUsed(DateTime lastUsedDate)
        {
            LastUsedDate = lastUsedDate;
        }

        public void RevokeAndMarkUsed()
        {
            IsRevoked = true;
            IsUsed = true;
        }
    }
}

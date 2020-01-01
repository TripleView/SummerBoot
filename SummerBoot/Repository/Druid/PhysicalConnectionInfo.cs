using System.Data;

namespace SummerBoot.Repository.Druid
{
    public sealed class PhysicalConnectionInfo
    {
        public IDbConnection DbConnection { get; }
        public long ConnectStartNanos { get; }
        public long ConnectedNanos { get; }

        public long InitedNanos { get; }

        public long ValidatedNanos { get; }

        public long ConnectNanoSpan => ConnectedNanos - ConnectStartNanos;

        public PhysicalConnectionInfo(IDbConnection dbConnection, long connectStartNanos, long connectedNanos, long initedNanos, long validatedNanos)
        {
            this.DbConnection = dbConnection;
            this.ConnectStartNanos = connectStartNanos;
            this.ConnectedNanos = connectedNanos;
            this.InitedNanos = initedNanos;
            this.ValidatedNanos = validatedNanos;
        }
    }
}
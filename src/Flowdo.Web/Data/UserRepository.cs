    using Dapper;
using Npgsql;

namespace Flowdo.Web.Data;

public sealed class UserRepository(
    NpgsqlDataSource dataSource)
{
    public async Task<Guid> UpsertAsync(
        string googleSub,
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO users (
                google_sub,
                email,
                display_name
            )
            VALUES (
                @GoogleSub,
                @Email,
                @DisplayName
            )
            ON CONFLICT (google_sub)
            DO UPDATE SET
                email = EXCLUDED.email,
                display_name = EXCLUDED.display_name,
                updated_at = NOW()
            RETURNING id;
            """;

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken
            );

        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                sql,
                new
                {
                    GoogleSub = googleSub,
                    Email = email,
                    DisplayName = displayName
                },
                cancellationToken: cancellationToken
            )
        );
    }
}
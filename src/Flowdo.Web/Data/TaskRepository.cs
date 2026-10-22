using Dapper;
using Npgsql;
using Flowdo.Web.Models;

namespace Flowdo.Web.Data;

public sealed class TaskRepository(
    NpgsqlDataSource dataSource)
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id AS "Id",
                user_id AS "UserId",
                title AS "Title",
                description AS "Description",
                status AS "Status",
                has_started AS "HasStarted",
                created_at AS "CreatedAt",
                updated_at AS "UpdatedAt",
                completed_at AS "CompletedAt"
            FROM tasks
            WHERE user_id = @UserId
            ORDER BY created_at DESC;
            """;

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken
            );

        var tasks = await connection.QueryAsync<TaskItem>(
            new CommandDefinition(
                sql,
                new { UserId = userId },
                cancellationToken: cancellationToken
            )
        );

        return tasks.ToList();
    }

    public async Task<Guid> CreateAsync(
        Guid userId,
        string title,
        string? description,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO tasks (
                user_id,
                title,
                description
            )
            VALUES (
                @UserId,
                @Title,
                @Description
            )
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
                    UserId = userId,
                    Title = title,
                    Description = description
                },
                cancellationToken: cancellationToken
            )
        );
    }
}
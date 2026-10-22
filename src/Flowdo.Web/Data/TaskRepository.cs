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
    public async Task<TaskItem?> GetEditableByIdAsync(
    Guid taskId,
    Guid userId)
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
        WHERE id = @TaskId
          AND user_id = @UserId
          AND status != 'done';
        """;

    await using var connection =
        await dataSource.OpenConnectionAsync();

    return await connection.QuerySingleOrDefaultAsync<TaskItem>(
        sql,
        new { TaskId = taskId, UserId = userId }
    );
}
public async Task<bool> UpdateAsync(
    Guid taskId,
    Guid userId,
    string title,
    string? description)
{
    const string sql = """
        UPDATE tasks
        SET
            title = @Title,
            description = @Description,
            status = 'todo',
            updated_at = NOW(),
            completed_at = NULL
        WHERE id = @TaskId
          AND user_id = @UserId
          AND status IN ('todo', 'in_progress');
        """;

    await using var connection =
        await dataSource.OpenConnectionAsync();

    var affected = await connection.ExecuteAsync(
        sql,
        new
        {
            TaskId = taskId,
            UserId = userId,
            Title = title,
            Description = description
        }
    );

    return affected == 1;
}
public async Task<bool> ChangeStatusAsync(
    Guid taskId,
    Guid userId,
    string nextStatus)
{
    const string sql = """
        UPDATE tasks
        SET
            status = @NextStatus,
            has_started = TRUE,
            updated_at = NOW(),
            completed_at = CASE
                WHEN @NextStatus = 'done'
                    THEN NOW()
                ELSE NULL
            END
        WHERE id = @TaskId
          AND user_id = @UserId
          AND (
              (
                  status = 'todo'
                  AND @NextStatus IN ('in_progress', 'done')
              )
              OR
              (
                  status = 'in_progress'
                  AND @NextStatus = 'done'
              )
          );
        """;

    await using var connection =
        await dataSource.OpenConnectionAsync();

    var affected = await connection.ExecuteAsync(
        sql,
        new
        {
            TaskId = taskId,
            UserId = userId,
            NextStatus = nextStatus
        }
    );

    return affected == 1;
}
public async Task<bool> DeleteAsync(
    Guid taskId,
    Guid userId)
{
    const string sql = """
        DELETE FROM tasks
        WHERE id = @TaskId
          AND user_id = @UserId
          AND status = 'todo'
          AND has_started = FALSE;
        """;

    await using var connection =
        await dataSource.OpenConnectionAsync();

    var affected = await connection.ExecuteAsync(
        sql,
        new
        {
            TaskId = taskId,
            UserId = userId
        }
    );

    return affected == 1;
}
}
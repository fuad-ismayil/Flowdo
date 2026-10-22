CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    google_sub VARCHAR(255) NOT NULL UNIQUE,
    email TEXT NOT NULL,
    display_name VARCHAR(160) NOT NULL,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE tasks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    user_id UUID NOT NULL REFERENCES users(id),

    title VARCHAR(160) NOT NULL,
    description TEXT,

    status VARCHAR(20) NOT NULL DEFAULT 'todo',
    has_started BOOLEAN NOT NULL DEFAULT FALSE,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMPTZ,

    CONSTRAINT chk_task_status CHECK (
        status IN ('todo', 'in_progress', 'done')
    ),

    CONSTRAINT chk_task_title CHECK (
        LENGTH(TRIM(title)) > 0
    ),

    CONSTRAINT chk_task_description CHECK (
        description IS NULL
        OR LENGTH(description) <= 3000
    ),

    CONSTRAINT chk_task_started CHECK (
        status = 'todo' OR has_started = TRUE
    )
);

CREATE INDEX idx_tasks_user_status
ON tasks(user_id, status, created_at DESC);
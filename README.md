# Flowdo

A modern personal task management application built with
ASP.NET Core MVC, Dapper, PostgreSQL, and Google OAuth.

## Features

- Google OAuth authentication
- Personal user workspace
- Create, edit, and manage tasks
- Three statuses: To Do, In Progress, Done
- Task ownership and authorization
- Protected task deletion rules
- Responsive Kanban dashboard
- Progress statistics

## Tech Stack

- ASP.NET Core 10 MVC
- C#
- Dapper
- Npgsql
- PostgreSQL
- Google OAuth 2.0
- Razor Views
- HTML, CSS, JavaScript

## Architecture

The application uses a SQL-first approach.

Database tables are created using SQL scripts.
Dapper executes parameterized SQL queries.

Entity Framework Core is not used.

## Project Structure

- `src/Flowdo.Web/Controllers` - HTTP requests
- `src/Flowdo.Web/Models` - Data models
- `src/Flowdo.Web/ViewModels` - Form validation
- `src/Flowdo.Web/Data` - Dapper repositories
- `src/Flowdo.Web/Views` - Razor pages
- `database` - PostgreSQL schema

## Getting Started

### Requirements

- .NET 10 SDK
- PostgreSQL
- Google Cloud OAuth Client

### Database

Create an empty PostgreSQL database.

Run the SQL script:

`database/001_init.sql`

### Configuration

Configure these values using .NET User Secrets:

- `ConnectionStrings:Default`
- `Authentication:Google:ClientId`
- `Authentication:Google:ClientSecret`

Register the OAuth redirect URI in Google Cloud Console:

`https://localhost:<YOUR_HTTPS_PORT>/signin-google`

### Run

```bash
dotnet restore
dotnet build
dotnet run --project src/Flowdo.Web --launch-profile https
```

## Task Rules

- New tasks start in To Do.
- Tasks can move to In Progress and Done.
- Editing resets a task to To Do.
- Completed tasks cannot be edited or deleted.
- Only untouched To Do tasks can be deleted.
- Users can only access their own tasks.

## Future Roadmap

- Task search and filtering
- Priorities and deadlines
- Team workspaces
- Task assignments
- Notifications
- Redis caching

## License

License to be determined.

## Author

Developed by **Fuad İsmayıl**.

GitHub: [@fuad-ismayil](https://github.com/fuad-ismayil)

Made with ❤️
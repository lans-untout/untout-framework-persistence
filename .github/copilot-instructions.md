# GitHub Copilot Instructions

This file contains instructions for GitHub Copilot when working in this repository.

## Commit Messages

When asked to generate commit messages:
- **Always use the shorter, concise version**
- Follow Conventional Commits format: `type(scope): brief description`
- Do NOT offer multiple versions or alternatives
- Keep the message body focused and under 5 lines
- Include only essential technical details

### Example Format:
```
refactor(scope): brief one-line summary

Short explanation of what changed and why (2-3 lines max).
List key changes as bullet points if needed.

Trade-off or note (1 line if relevant).
```

## Code Style

- Follow existing patterns in the codebase
- Use C# 12 features when appropriate (.NET 8)
- Prefer explicit null checks with `ArgumentNullException.ThrowIfNull`
- Use nullable reference types consistently
- Keep methods focused and single-purpose

## Architecture Principles

- Separation of concerns: Repository (business logic) vs Infrastructure (connections, logging)
- Dependency injection friendly
- Test-driven design (all abstractions mockable)
- Connection pooling via short-lived connections
- Logging optional but pluggable

## Testing

- Use xUnit for all tests
- Mock interfaces, not implementations
- Test edge cases (null inputs, failed operations)
- Verify logging calls in integration tests

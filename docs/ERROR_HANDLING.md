# Error Handling Documentation

## Strategy
FlowSpace uses a centralized error handling strategy to ensure consistency, security, and a great developer experience.

## Backend (ASP.NET Core)
- **Middleware**: A custom `ExceptionHandlingMiddleware` catches all unhandled exceptions.
- **Result Pattern**: Use a `Result<T>` or `OneOf` pattern in the Application layer to return expected failures (e.g., `NotFound`, `ValidationFailed`) instead of throwing exceptions.
- **Problem Details**: Return errors following the [RFC 7807](https://tools.ietf.org/html/rfc7807) standard.

### Error Categories
- **400 Bad Request**: Validation failures (FluentValidation).
- **401 Unauthorized**: Missing or invalid JWT.
- **403 Forbidden**: User lacks permission for the specific resource.
- **404 Not Found**: Resource does not exist.
- **500 Internal Server Error**: Unexpected system failures (logged with full stack trace, but hidden from user).

## Frontend (Next.js)
- **Error Boundaries**: React Error Boundaries at the Page and Component levels to prevent total app crashes.
- **Toasts**: User-friendly error notifications using Shadcn UI Toast component.
- **Graceful Degradation**: If real-time sync fails, the UI falls back to "Offline Mode" and notifies the user.

## Logging
- **Correlation ID**: Every request gets a unique ID, passed through all services and logs.
- **Sensitive Data**: Automatic masking of PII (Emails, Passwords) in log outputs.

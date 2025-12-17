# OpenID Connect Test Server

A lightweight OpenID Connect server for testing OIDC client implementations. Built with .NET 10 and OpenIddict.

## Features

- **No database required** - file-based configuration for users and clients
- **User picker UI** - select which test user to authenticate as (no passwords)
- **Docker ready** - run with a single command
- **Standard OIDC flows** - Authorization Code + PKCE, Client Credentials, Refresh Tokens

## Quick Start

```bash
docker compose up
```

Server available at **http://localhost:50000**

## Endpoints

| Endpoint | URL |
|----------|-----|
| Discovery | http://localhost:50000/.well-known/openid-configuration |
| Authorization | http://localhost:50000/connect/authorize |
| Token | http://localhost:50000/connect/token |
| UserInfo | http://localhost:50000/connect/userinfo |
| End Session | http://localhost:50000/connect/logout |

## Configuration

### Users (`config/users.json`)

```json
{
  "users": [
    {
      "id": "1",
      "username": "alice",
      "email": "alice@example.com",
      "claims": {
        "name": "Alice Smith",
        "role": "admin",
        "department": "Engineering"
      }
    }
  ]
}
```

Add any custom claims - they'll be included in tokens.

### Clients (`config/clients.json`)

```json
{
  "clients": [
    {
      "clientId": "my-app",
      "clientSecret": "my-secret",
      "displayName": "My Application",
      "redirectUris": ["http://localhost:3000/callback"],
      "postLogoutRedirectUris": ["http://localhost:3000/"],
      "permissions": ["authorization_code", "refresh_token", "openid", "profile", "email"]
    }
  ]
}
```

Omit `clientSecret` for public clients (SPAs, mobile apps).

#### Available Permissions

- `authorization_code` - Authorization Code flow
- `client_credentials` - Client Credentials flow
- `refresh_token` - Refresh tokens
- `openid`, `profile`, `email` - Standard OIDC scopes
- `api` - Custom API scope

## Default Test Users

| Username | Role | Department |
|----------|------|------------|
| alice | admin | Engineering |
| bob | user | Sales |
| carol | manager | Marketing |

## Default Test Clients

| Client ID | Secret | Type |
|-----------|--------|------|
| `test-client` | `test-secret` | Confidential (web apps) |
| `public-client` | - | Public (SPA/mobile) |
| `machine-client` | `machine-secret` | Client Credentials |

## Examples

### Client Credentials Flow

```bash
curl -X POST http://localhost:50000/connect/token \
  -d "grant_type=client_credentials" \
  -d "client_id=machine-client" \
  -d "client_secret=machine-secret" \
  -d "scope=api"
```

### Authorization Code Flow

Open in browser:
```
http://localhost:50000/connect/authorize?client_id=test-client&redirect_uri=http://localhost:5001/callback&response_type=code&scope=openid%20profile%20email
```

Select a user, then exchange the code:
```bash
curl -X POST http://localhost:50000/connect/token \
  -d "grant_type=authorization_code" \
  -d "client_id=test-client" \
  -d "client_secret=test-secret" \
  -d "code=YOUR_CODE" \
  -d "redirect_uri=http://localhost:5001/callback"
```

### Postman

- Auth URL: `http://localhost:50000/connect/authorize`
- Token URL: `http://localhost:50000/connect/token`
- Callback: `https://oauth.pstmn.io/v1/callback`

## Running Without Docker

```bash
cd src/TestOidcServer
dotnet run
```

## License

MIT

# OpenID Connect Test Server

A lightweight OpenID Connect server for testing OIDC client implementations. Built with .NET 10 and OpenIddict.

## Features

- **No database required** - file-based configuration for users and clients
- **User picker UI** - select which test user to authenticate as (no passwords)
- **Standard OIDC flows** - Authorization Code, Client Credentials, Refresh Tokens
- **Includes test client** - ready-to-use web app for verifying the setup

## Quick Start

```bash
./dev.sh
```

This starts both the OIDC server and test client in a tmux session.

Or run manually:

```bash
# Terminal 1: OIDC Server
cd src/TestOidcServer && dotnet run

# Terminal 2: Test Client
cd src/TestClient && dotnet run
```

- **OIDC Server**: http://localhost:50000
- **Test Client**: http://localhost:60000

## Test Client

The included test client (`src/TestClient`) demonstrates OIDC authentication:

- **Home page** (`/`) - Shows login status
- **Private page** (`/Private`) - Requires authentication, displays user claims and tokens
- **Logout** (`/Logout`) - Signs out from both client and OIDC server

Visit http://localhost:60000 and click "Login to View Profile" to test the flow.

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

## License

MIT

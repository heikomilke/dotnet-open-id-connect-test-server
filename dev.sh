#!/bin/bash

SESSION="oidc-dev"

# Kill existing session if it exists
tmux kill-session -t $SESSION 2>/dev/null

# Create new session with OIDC server
tmux new-session -d -s $SESSION -n "servers" "cd src/TestOidcServer && dotnet run"

# Split horizontally and run test client
tmux split-window -h -t $SESSION "cd src/TestClient && dotnet run"

# Bind PREFIX + k to kill the session
tmux bind-key -T prefix k kill-session

# Attach to session
tmux attach-session -t $SESSION

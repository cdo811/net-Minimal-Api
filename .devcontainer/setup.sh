#!/bin/bash
set -e

echo "🔧 Running postCreateCommand setup..."

if [ -f "frontend/package.json" ]; then
  echo "📦 Installing npm dependencies..."
  npm install --prefix frontend
else
  echo "⚠️  Skipping npm install: frontend/package.json not found"
fi

if [ -f "fastapi_service/requirements.txt" ]; then
  echo "🐍 Installing Python dependencies..."
  pip install -r fastapi_service/requirements.txt
else
  echo "⚠️  Skipping pip install: fastapi_service/requirements.txt not found"
fi

if [ -f "test.csproj" ]; then
  echo "🔵 Restoring .NET dependencies..."
  dotnet restore test.csproj
else
  echo "⚠️  Skipping dotnet restore: test.csproj not found"
fi

echo "✅ Setup complete!"

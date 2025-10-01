#!/bin/bash

# Set DB path from CHIRPDBPATH, or fall back to /tmp/chirp.db
DB_PATH="${CHIRPDBPATH:-/tmp/chirp.db}"

# Paths to your SQL files
SCHEMA="src/Chirp.Razor/Data/schema.sql"
DUMP="src/Chirp.Razor/Data/dump.sql"

# Check if files exist
if [[ ! -f "$SCHEMA" || ! -f "$DUMP" ]]; then
  echo "schema or dump not found."
  exit 1
fi

echo "Creating database at $DB_PATH..."

sqlite3 "$DB_PATH" < "$SCHEMA"
sqlite3 "$DB_PATH" < "$DUMP"

echo "Database is all good"

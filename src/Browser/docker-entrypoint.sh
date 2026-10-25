#!/bin/sh
set -e

if [ -z "$MAYFLY_SERVER" ]; then
    echo "MAYFLY_SERVER is not set: the address of the Mayfly API, for example https://api.example.com" >&2
    exit 1
fi

printf '{ "server": "%s" }\n' "$MAYFLY_SERVER" > /usr/share/nginx/html/config.json

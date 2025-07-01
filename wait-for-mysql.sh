#!/bin/bash
set -e

host="$1"
shift
cmd="$@"

echo "Esperando a que MySQL ($host) esté disponible..."

until mysql -h "$host" -u root -p0322 -e 'select 1;' &> /dev/null; do
  >&2 echo "MySQL no está listo todavía - esperando..."
  sleep 3
done

echo "MySQL disponible - ejecutando comando: $cmd"
exec $cmd

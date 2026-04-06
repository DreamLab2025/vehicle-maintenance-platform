MAX_VOLUME_PASSES=3
SLEEP_BETWEEN_VOLUME_PASSES_SEC=0.4

for c in \
  verendar-aspire-postgres verendar-aspire-rabbitmq verendar-aspire-redis \
  verendar-aspire-seq verendar-aspire-gateway verendar-aspire-pgadmin \
  PostgresDb Rabbitmq ApiGateway PgAdmin
do
  docker rm -fv "$c" 2>/dev/null || true
done

# Dynamic names from apphost runs
docker ps -a --format '{{.Names}}' | while read -r name; do
  case "$name" in
    redis-cache-*|seq-*) docker rm -fv "$name" 2>/dev/null || true ;;
  esac
done

i=0
while [ "$i" -lt "$MAX_VOLUME_PASSES" ]; do
  docker volume ls -q | while read -r vol; do
    case "$vol" in
      *verendar.apphost*) docker volume rm -f "$vol" 2>/dev/null || true ;;
    esac
  done
  i=$((i + 1))
  sleep "$SLEEP_BETWEEN_VOLUME_PASSES_SEC"
done

echo 'App cleanup finished. If something remains, stop AppHost and run again.'

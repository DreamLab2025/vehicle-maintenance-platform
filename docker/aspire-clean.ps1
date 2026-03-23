# Stops/removes Aspire dev containers and all related Docker volumes (named + anonymous on those containers).
# Stop the AppHost process (Ctrl+C) first so containers are not recreated.

$ErrorActionPreference = 'Continue'

$known = @(
    'verendar-aspire-postgres',
    'verendar-aspire-rabbitmq',
    'verendar-aspire-redis',
    'verendar-aspire-seq',
    'verendar-aspire-gateway',
    'verendar-aspire-pgadmin',
    'PostgresDb',
    'Rabbitmq',
    'ApiGateway',
    'PgAdmin'
)
foreach ($c in $known) {
    docker rm -fv $c 2>$null | Out-Null
}

docker ps -a --format '{{.Names}}' | ForEach-Object {
    if ($_ -match '^redis-cache-' -or $_ -match '^seq-') {
        docker rm -fv $_ 2>$null | Out-Null
    }
}

# Named Aspire volumes (repeat: Docker can briefly hold locks after rm)
for ($i = 0; $i -lt 3; $i++) {
    docker volume ls -q | Where-Object { $_ -match 'verendar\.apphost' } | ForEach-Object {
        docker volume rm -f $_ 2>$null | Out-Null
    }
    Start-Sleep -Milliseconds 400
}

Write-Host 'Aspire Docker cleanup finished (containers + volumes). If something remains, stop AppHost and run again.'

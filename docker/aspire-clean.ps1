$ErrorActionPreference = 'Continue'

$MaxVolumePasses = 3
$SleepBetweenVolumePassesMs = 400

$KnownContainers = @(
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

foreach ($c in $KnownContainers) {
    docker rm -fv $c 2>$null | Out-Null
}

docker ps -a --format '{{.Names}}' | ForEach-Object {
    if ($_ -match '^redis-cache-' -or $_ -match '^seq-') {
        docker rm -fv $_ 2>$null | Out-Null
    }
}

for ($i = 0; $i -lt $MaxVolumePasses; $i++) {
    docker volume ls -q | Where-Object { $_ -match 'verendar\.apphost' } | ForEach-Object {
        docker volume rm -f $_ 2>$null | Out-Null
    }
    Start-Sleep -Milliseconds $SleepBetweenVolumePassesMs
}

Write-Host 'App cleanup finished. If something remains, stop AppHost and run again.'

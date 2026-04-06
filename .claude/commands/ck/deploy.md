Deploy the Verendar backend.

## Local development

```bash
task run        # start all services + infra via Aspire
task build      # build solution only
task clear      # remove Aspire containers and volumes
```

Migrations and seeding run automatically on startup (dev only).

## Production (EC2 + Docker Compose)

All services run as Docker containers behind Nginx gateway on EC2. Deploy steps from the `Docker/` directory:

```bash
# SSH into EC2
ssh -i "your-key.pem" ec2-user@<EC2_PUBLIC_IP>

cd /opt/verendar/Docker

# Pull latest images and restart
docker-compose -f docker-compose.prod.yml --env-file .env.prod pull
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d

# Verify
docker-compose -f docker-compose.prod.yml --env-file .env.prod ps
curl -s http://localhost/health
```

### Environment variables (`.env.prod`)
Key vars to confirm before deploy:
- `GATEWAY_PORT=80`
- `JWT_BEARER_ISSUER` / `JWT_BEARER_AUDIENCE` = `https://api.verendar.vn`
- `CORS_ALLOWED_ORIGINS` = frontend domains
- Per-service DB names: `IDENTITY_SERVICE_DATABASE`, `VEHICLE_SERVICE_DATABASE`, etc.

### Verify end-to-end

```bash
curl -s https://api.verendar.vn/health   # → healthy
```

Seq logs accessible via SSH tunnel only: `ssh -L 5340:localhost:5340 ec2-user@<EC2_IP>` → open `http://localhost:5340`.

## If something goes wrong

- Check container logs: `docker-compose -f docker-compose.prod.yml --env-file .env.prod logs -f <service-name>`
- Confirm DB reachable and `.env.prod` has correct credentials
- Gateway not responding → check `GATEWAY_PORT=80` in `.env.prod` and Security Group allows port 80
- Cloudflare SSL issue → set SSL/TLS mode to **Full** (not Strict) in Cloudflare Dashboard

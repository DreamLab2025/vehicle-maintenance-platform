# GitHub Actions Workflows

## Docker Build & Push (`docker-deploy.yml`)

Build và push image của 5 service (Identity, Vehicle, Media, Notification, Ai) lên Docker Hub khi push lên `main` hoặc chạy thủ công.

### Cấu hình Secrets (Settings → Secrets and variables → Actions)

| Secret | Mô tả |
|--------|--------|
| `DOCKER_USERNAME` | Tên đăng nhập Docker Hub (hoặc org) |
| `DOCKER_PASSWORD` | Docker Hub Access Token (Account Settings → Security → New Access Token, quyền Read & Write) |

### Caching

Workflow dùng Docker Buildx với **GitHub Actions cache** (`type=gha`): layer được cache giữa các lần chạy, chỉ layer thay đổi mới build lại → giảm thời gian build.

### Tag image

- Push lên `main`/`master`: tag = `sha-<7 ký tự commit>` và `latest`.
- Trên server, trong `.env.prod` đặt `DOCKER_IMAGE_PREFIX=<DOCKER_USERNAME>` và `IMAGE_TAG=latest` (hoặc tag cụ thể).

### Chạy thủ công

Actions → Docker Build & Push → Run workflow.

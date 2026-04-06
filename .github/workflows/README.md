# GitHub Actions Workflows

## Docker Build & Push (`docker-deploy.yml`)

Build và push image của 7 service (Identity, Vehicle, Media, Notification, Ai, Location, Garage) lên Docker Hub.

**Trigger:** push nhánh `main`, push tag `v*` (semver release), hoặc `workflow_dispatch`.

**Tối ưu mới:**

- Bỏ qua run khi chỉ thay đổi docs/markdown.
- Tự động phát hiện service thay đổi để chỉ build image cần thiết.
- Nếu thay đổi phần dùng chung (`App/**`, contracts, solution, workflow), workflow sẽ build toàn bộ image.
- Hủy run cũ cùng nhánh/tag khi có push mới (concurrency).
- Cache NuGet cho bước verify và cache Docker layer theo từng service.

**Luồng:** job `changes` xác định matrix cần build; nếu có thay đổi deployable thì job `verify` chạy `dotnet restore` + `dotnet build Verendar.sln -c Release`, sau đó mới chạy các job build Docker **song song** theo matrix.

**Lưu ý:** Workflow chỉ build/push image — **không** deploy tự động lên server. Trên server vẫn `docker compose pull` / `up` như hiện tại.

### Cấu hình Secrets (Settings → Secrets and variables → Actions)

| Secret            | Mô tả                                                                                        |
| ----------------- | -------------------------------------------------------------------------------------------- |
| `DOCKER_USERNAME` | Tên đăng nhập Docker Hub (hoặc org)                                                          |
| `DOCKER_PASSWORD` | Docker Hub Access Token (Account Settings → Security → New Access Token, quyền Read & Write) |

### Caching

Workflow dùng Docker Buildx với **GitHub Actions cache** (`type=gha`) và NuGet cache trong bước .NET verify: layer/package được tái sử dụng giữa các lần chạy để giảm thời gian CI.

### Tag image

- Push lên `main`: tag = `sha-<7 ký tự commit>` và `latest`.
- Push tag `v1.2.3`: image cũng tag `v1.2.3` và `latest`.
- Trên server, trong `.env.prod` đặt `DOCKER_IMAGE_PREFIX=<DOCKER_USERNAME>` và `IMAGE_TAG=latest` (hoặc tag cụ thể).

### Skip logic

- Nếu commit không chạm vào service hoặc phần deployable, workflow sẽ kết thúc với job `Skip build`.
- `workflow_dispatch` luôn build toàn bộ image (phù hợp khi cần phát hành thủ công).

### Chạy thủ công

Actions → Docker Build & Push → Run workflow.

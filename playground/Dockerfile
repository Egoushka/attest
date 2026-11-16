# syntax=docker/dockerfile:1
# Build context is the repo root: docker build -f playground/Dockerfile .
# Native AOT publish on the SDK image with the AOT toolchain, run on a chiseled runtime-deps base (no shell, no package
# manager, non-root). The Compose healthcheck runs `Attest.Playground --healthcheck`. Both bases are pinned by index
# digest and the FROM lines stay literal so Dependabot (docker) can bump them.
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble-aot@sha256:7d223c2b945173edb8c73e5f5d48a64e7cd0ca632ccf95170b857e8eea78913c AS build
ARG TARGETARCH
WORKDIR /src
COPY Directory.Build.props ./
COPY Attest/ Attest/
COPY playground/Attest.Playground/ playground/Attest.Playground/
RUN case "$TARGETARCH" in \
      amd64) echo linux-x64 ;; \
      arm64) echo linux-arm64 ;; \
      *) echo "unsupported TARGETARCH: $TARGETARCH" >&2; exit 1 ;; \
    esac > /tmp/rid
RUN dotnet publish playground/Attest.Playground/Attest.Playground.csproj -c Release -r "$(cat /tmp/rid)" -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled@sha256:40eba5db69594d517c3a61e96bdacc2e3d6f8c99a44b52d55edee844d33518d0 AS runtime
WORKDIR /app
COPY --from=build /app/publish/Attest.Playground ./Attest.Playground
ENV ASPNETCORE_HTTP_PORTS=8080 DOTNET_EnableDiagnostics=0
EXPOSE 8080
ENTRYPOINT ["/app/Attest.Playground"]

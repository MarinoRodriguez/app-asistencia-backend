IMAGE ?= marinorodriguez/app-asistencia-frontend
VERSION ?= v1.0.0
PLATFORMS ?= linux/amd64,linux/arm64
BUILDER ?= multiarch_builder

.PHONY: buildx-init docker-push

buildx-init:
	@docker buildx create --name $(BUILDER) --use || docker buildx use $(BUILDER)

# Build and push multi-arch image
# Usage: make docker-push VERSION=v1.0.0

docker-push: buildx-init
	@docker buildx build \
		--platform $(PLATFORMS) \
		-t $(IMAGE):$(VERSION) \
		-t $(IMAGE):latest \
		--push \
		.

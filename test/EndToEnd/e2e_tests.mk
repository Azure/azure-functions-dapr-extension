E2E_TEST_APPS_DIR := ./test/EndToEnd/Apps

# E2E_TEST_APPS contains a list of all e2e test apps.
# This list is used to build and push all e2e test app images.
E2E_TEST_APPS=$(shell ls $(E2E_TEST_APPS_DIR))

# check the required environment variables
check-e2e-env:
ifeq ($(E2E_TEST_APP_REGISTRY),)
	$(error E2E_TEST_APP_REGISTRY environment variable must be set)
endif
ifeq ($(E2E_TEST_APP_TAG),)
	$(error E2E_TEST_APP_TAG environment variable must be set)
endif

define genTestAppImageBuild
.PHONY: build-e2e-app-$(1)
build-e2e-app-$(1): check-e2e-env
	@echo "Building e2e test app $(1) image"
	docker build -t $(E2E_TEST_APP_REGISTRY)/$(1):$(E2E_TEST_APP_TAG) -f $(E2E_TEST_APPS_DIR)/$(1)/Dockerfile .
endef

# Generate test app image build targets
$(foreach ITEM,$(E2E_TEST_APPS),$(eval $(call genTestAppImageBuild,$(ITEM))))

# Enumerate test app build targets
BUILD_E2E_APPS_TARGETS:=$(foreach ITEM,$(E2E_TEST_APPS),build-e2e-app-$(ITEM))

# Build all e2e test app images
build-e2e-app-all: $(BUILD_E2E_APPS_TARGETS)

define genTestAppImagePush
.PHONY: push-e2e-app-$(1)
push-e2e-app-$(1): check-e2e-env
	@echo "Pushing e2e test app $(1) image"
	docker push $(E2E_TEST_APP_REGISTRY)/$(1):$(E2E_TEST_APP_TAG)
endef

# Generate test app image push targets
$(foreach ITEM,$(E2E_TEST_APPS),$(eval $(call genTestAppImagePush,$(ITEM))))

# Enumerate test app push targets
PUSH_E2E_APPS_TARGETS:=$(foreach ITEM,$(E2E_TEST_APPS),push-e2e-app-$(ITEM))

# Push all e2e test app images
push-e2e-app-all: $(PUSH_E2E_APPS_TARGETS)
# Variables
CONFIGURATION ?= debug

# Default target
all: build

# Build target
build:
	dotnet build --configuration $(CONFIGURATION) --configfile nuget.config

# Test target
test:
	dotnet test --configuration $(CONFIGURATION)

# Clean target
clean:
	dotnet clean

# Phony targets
.PHONY: all build test clean
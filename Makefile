# Variables
CONFIGURATION ?= debug

# Default target
all: build

# Build target
build:
	dotnet build --configuration $(CONFIGURATION) --configfile nuget.config

# Clean target
clean:
	dotnet clean

# Phony targets
.PHONY: all build clean

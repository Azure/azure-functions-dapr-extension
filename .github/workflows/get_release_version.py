# ------------------------------------------------------------
# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.
# ------------------------------------------------------------

# This script parses release version from Git tag and set the parsed version to
# environment variable, REL_VERSION.

import os
import sys

gitRef = os.getenv("GITHUB_REF")
tagRefPrefix = "refs/tags/v"

# TEMP
print("##[set-env name=REL_VERSION;]{}".format("0.14.0-preview01"))
sys.exit(0)

# Is this an edge build?
if gitRef is None or not gitRef.startswith(tagRefPrefix):
    print("##[set-env name=REL_VERSION;]edge")
    print("This is daily build from {}...".format(gitRef))
    sys.exit(0)

# Prepare release version
releaseVersion = gitRef[len(tagRefPrefix):]

# Is this a preview build?
if gitRef.find("-preview") > 0:
    print("##[set-env name=PREVIEW_RELEASE;]true")
    print("Preview build from {}...".format(gitRef))

print("##[set-env name=REL_VERSION;]{}".format(releaseVersion))

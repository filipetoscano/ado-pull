#!/bin/bash
# ------------------------------------------------------------------------
# Quality gates shared by ci.sh and release.sh.
#
# Assumes 'dotnet restore' has already run: every check here uses
# --no-restore, or relies on the restore assets being present.
# ------------------------------------------------------------------------
set -eux

yell() { echo "$0: $*" >&2; }
die() { yell "$*"; exit 111; }
try() { "$@" || die "cannot $*"; }


#
# Run all commands from the repository root!
# (That's the directory above the current one :)
# ------------------------------------------------------------------------
#
SCRIPT_PATH="${BASH_SOURCE[0]}"
if ([ -h "${SCRIPT_PATH}" ]); then
  while([ -h "${SCRIPT_PATH}" ]); do cd "$(dirname "$SCRIPT_PATH")";
  SCRIPT_PATH=$(readlink "${SCRIPT_PATH}"); done
fi
cd "$(dirname "${SCRIPT_PATH}")" > /dev/null
cd ..


#
# Prerequisites
# ------------------------------------------------------------------------
command -v jq > /dev/null || die "jq is required but was not found on PATH"


#
# Formatting
# ------------------------------------------------------------------------
dotnet format --verify-no-changes --no-restore


#
# Vulnerable packages
#
# 'dotnet list package --vulnerable' exits 0 whether or not it finds
# anything, so the JSON output has to be inspected to gate the build. In
# --vulnerable mode a project only carries a 'frameworks' key when at
# least one of its packages is vulnerable.
# ------------------------------------------------------------------------
mkdir -p tmp
dotnet list package --vulnerable --include-transitive --format json --output-version 1 > tmp/vulnerable.json

VULNERABLE=$(jq -r '[ .projects[]? | .frameworks[]? | (.topLevelPackages[]?, .transitivePackages[]?) ] | length' tmp/vulnerable.json)

if [ "${VULNERABLE}" != "0" ]; then
    jq -r '.projects[]? | .path as $p | .frameworks[]? | (.topLevelPackages[]?, .transitivePackages[]?)
           | "\($p): \(.id) \(.resolvedVersion) [\([ .vulnerabilities[]?.severity ] | join(", "))]"' tmp/vulnerable.json >&2
    die "found ${VULNERABLE} vulnerable package(s)"
fi

# eof

#!/bin/bash
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
cd "$(dirname ${SCRIPT_PATH})" > /dev/null
cd ..


#
# Build
# ------------------------------------------------------------------------
dotnet clean   -c Release
dotnet restore --packages .nuget --locked-mode

# Invoked through bash rather than as ./cicd/checks.sh: core.filemode is
# false on Windows, so the executable bit does not reliably survive a commit.
bash cicd/checks.sh

dotnet build   -c Release --no-restore


#
# Test
#
# Runs after the build rather than alongside the checks, since --no-build
# needs the Release output to already exist.
#
# --report-trx writes one TestResults/<project>.trx per test project. Nothing
# consumes them yet: they are here so that publishing them -- as an artifact,
# or as annotations on a pull request -- is a workflow change rather than a
# change to what the build does.
# ------------------------------------------------------------------------
dotnet test    -c Release --no-restore --no-build --report-trx

# eof
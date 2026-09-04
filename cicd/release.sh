#!/bin/bash
# ------------------------------------------------------------------------
set -euxo pipefail

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
# Ensure env
# ------------------------------------------------------------------------
if [ -z ${GITHUB_REF+x} ];      then die "GITHUB_REF is not set"; fi
if [ -z ${GITHUB_TOKEN+x} ];    then die "GITHUB_TOKEN is not set"; fi
if [ -z ${NUGET_APIKEY+x} ];    then die "NUGET_APIKEY is not set"; fi

if [[ ! ${GITHUB_REF} =~ ^refs/tags/v[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
    die "GITHUB_REF is not a vN.N.N tag: ${GITHUB_REF}"
fi

export VERSION="${GITHUB_REF#refs/tags/v}"
echo "${VERSION}"


#
# Build
# ------------------------------------------------------------------------

dotnet clean   -c Release
dotnet restore --packages .nuget --locked-mode

bash cicd/checks.sh

dotnet build   -c Release --no-restore -p:Version=${VERSION}
dotnet test    -c Release --no-restore --no-build -p:Version=${VERSION}


#
# Package
# ------------------------------------------------------------------------

mkdir -p nupkg
rm -f nupkg/*.*


#
# Artifacts
# ------------------------------------------------------------------------

mkdir -p artifacts
rm -f artifacts/*.zip


#
# Release, including artifacts
#
# Everything reversible happens before the push to nuget.org: a release
# can be deleted and a tag re-cut, but a published package version is
# forever. Keep the irreversible step last.
# ------------------------------------------------------------------------

gh release create v${VERSION} --notes="Release v${VERSION}"


#
# Publish to nuget.org
# ------------------------------------------------------------------------

# NUGET_APIKEY must never reach the log: xtrace would echo the expanded
# command line, and masking is the action's job, not something to rely on.
set +x
dotnet nuget push "nupkg/*.nupkg" --api-key "${NUGET_APIKEY}" --source=https://api.nuget.org/v3/index.json
set -x

# eof
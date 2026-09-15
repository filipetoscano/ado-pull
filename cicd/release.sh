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
# Artifacts
# ------------------------------------------------------------------------

dotnet publish -c Release --runtime=win-x64   --self-contained tools/Lefty.Ado.Cli/Lefty.Ado.Cli.csproj -p:Version=${VERSION} -o tmp/win-x64
dotnet publish -c Release --runtime=linux-x64 --self-contained tools/Lefty.Ado.Cli/Lefty.Ado.Cli.csproj -p:Version=${VERSION} -o tmp/linux-x64
dotnet publish -c Release --runtime=osx-arm64 --self-contained tools/Lefty.Ado.Cli/Lefty.Ado.Cli.csproj -p:Version=${VERSION} -o tmp/osx-arm64

mkdir -p artifacts
rm -f artifacts/*.zip

zip -j -r  artifacts/adopull-win-x64-${VERSION}.zip    tmp/win-x64/adopull.exe
zip -j -r  artifacts/adopull-linux-x64-${VERSION}.zip  tmp/linux-x64/adopull
zip -j -r  artifacts/adopull-osx-arm64-${VERSION}.zip  tmp/osx-arm64/adopull


#
# Release, including artifacts
# ------------------------------------------------------------------------

gh release create v${VERSION} --notes="Release v${VERSION}" \
   artifacts/adopull-win-x64-${VERSION}.zip \
   artifacts/adopull-linux-x64-${VERSION}.zip \
   artifacts/adopull-osx-arm64-${VERSION}.zip

# eof
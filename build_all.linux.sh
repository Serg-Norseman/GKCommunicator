#!/bin/sh

# If you have mono x86 installed on a amd64 linux.
#export CFLAGS=-m64
#export CXXFLAGS=-m64
#export LDFLAGS=-m64
#export LD_LIBRARY_PATH=$PWD/external/lua/linux/lib64

xbuild ./GKCommunicator.sln /p:Configuration=Debug /p:Platform="x64" /p:MonoCS=true /p:TargetFrameworkVersion=v4.7.2 /v:quiet

export TEMP="$( mktemp -d )"
nunit-console ./GKNetCore.Tests/bin/Debug/GKNetCore.Tests.dll && rm -r "$TEMP"

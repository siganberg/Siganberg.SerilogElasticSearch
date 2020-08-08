#!/bin/bash

dotnet nuget push Siganberg.SirilogElasticSearch/bin/Release/Siganberg.SirilogElasticSearch.$1.nupkg -k $NUGETKEY -s "https://api.nuget.org/v3/index.json"

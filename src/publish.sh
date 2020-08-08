#!/bin/bash

dotnet nuget push Siganberg.SirilogElasticSearch/bin/Release/Siganberg.SirilogElasticSearch.1.0.1.nupkg -k $1 -s "https://api.nuget.org/v3/index.json"

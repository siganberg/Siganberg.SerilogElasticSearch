rm -rf test_results

dotnet build -c Release

dotnet test \
  --filter exclude!="true" \
  --no-build  \
  --logger:"trx;LogFileName=test-results.trx" \
  -c Release  \
  /p:CollectCoverage=\"true\"  \
  /p:ExcludeByAttribute=\"Obsolete,ExcludeFromCodeCoverage\"  \
  /p:CoverletOutput=\"../../test_results/\"  \
  /p:MergeWith=\"../../test_results/coverage.json\"  \
  /p:CoverletOutputFormat=\"json,opencover\" 
 
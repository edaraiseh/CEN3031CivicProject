#!/usr/bin/bash

dotnet test \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=coverage/ \
  /p:ExcludeByFile="**/Migrations/*.cs"

reportgenerator -reports:coverage/coverage.cobertura.xml -targetdir:coverage/coverage-report
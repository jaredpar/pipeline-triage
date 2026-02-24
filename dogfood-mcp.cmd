@echo off
set PIPELINE_ROOT=%~dp0
copy %PIPELINE_ROOT%\plugin\plugin-mcp.json %PIPELINE_ROOT%\artifacts\plugin.json
call copilot plugin uninstall pipeline-triage
call copilot plugin install %PIPELINE_ROOT%\artifacts
call copilot
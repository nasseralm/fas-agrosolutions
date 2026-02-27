@echo off
cd /d "%~dp0"
git add -A
git commit -m "feat: dashboard login talhoes grafico alertas contraste"
if %errorlevel% equ 0 (
  echo Commit feito com sucesso. Rode: git push
) else (
  echo Erro no commit. Verifique acima.
)

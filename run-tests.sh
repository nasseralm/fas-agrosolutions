#!/usr/bin/env bash
# Testes unitários — obrigatório antes do deploy local
# Uso: ./run-tests.sh   (na raiz do repositório)
# Sai com código 1 se algum teste falhar.

cd "$(dirname "$0")"
FAILED=0

SOLUTIONS=(
  "FAS-Usuarios/FAS.sln"
  "FAS-Propriedades/FAS.sln"
  "FAS-DataReceiver/Agro.DataReceiver.sln"
)

echo "Executando testes unitários (deploy local exige que passem)..."
for sln in "${SOLUTIONS[@]}"; do
  [ -f "$sln" ] || continue
  echo ""
  echo "  Test: $sln"
  if ! dotnet test "$sln" --configuration Release --verbosity minimal --nologo; then
    FAILED=1
  fi
done

echo ""
if [ "$FAILED" -ne 0 ]; then
  echo "Falha: um ou mais projetos de teste falharam. Corrija antes do deploy local."
  exit 1
fi
echo "Todos os testes passaram. Pode prosseguir com o deploy local (ex.: docker compose up -d)."
exit 0

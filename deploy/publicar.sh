#!/usr/bin/env bash
# =====================================================================
# publicar.sh — se ejecuta EN TU EQUIPO.
#   1. Corre las pruebas.
#   2. Publica la app para linux-x64 y genera app-linux.zip (con VERSION).
#   3. Copia al recurso compartido del servidor: app-linux.zip,
#      Dockerfile.menusemanal y actualizar-menusemanal.sh
#      (el zip anterior se conserva como app-linux.prev.zip).
#
# Uso:  deploy/publicar.sh [carpeta-destino]
#   Por defecto usa el recurso SMB montado por GVFS.
# =====================================================================
set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DESTINO="${1:-/run/user/$(id -u)/gvfs/smb-share:server=192.168.2.22,share=data/aplicaciones-web/MenuSemanal}"

die()  { echo "ERROR: $*" >&2; exit 1; }
paso() { echo; echo ">>> $*"; }

[ -d "$DESTINO" ] || die "no encuentro $DESTINO (¿está montado el recurso SMB? ábrelo en el gestor de archivos)"

if [ -n "$(git -C "$REPO" status --porcelain)" ]; then
  echo "AVISO: hay cambios sin commit; la versión publicada no coincidirá con ningún commit."
  SUFIJO="-dirty"
else
  SUFIJO=""
fi
VERSION="$(date +%Y%m%d-%H%M)-$(git -C "$REPO" rev-parse --short HEAD)$SUFIJO"

paso "1. Pruebas"
dotnet test "$REPO/MenuSemanal.sln" --nologo -v quiet || die "las pruebas fallaron; no se publica"

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

paso "2. Publicando $VERSION (Release, linux-x64)"
dotnet publish "$REPO/MenuSemanal.csproj" -c Release -r linux-x64 --self-contained false \
  -o "$TMP/app" --nologo -v quiet
echo "$VERSION" > "$TMP/app/VERSION"

for f in MenuSemanal.dll libe_sqlite3.so Data/comidas.json VERSION; do
  [ -f "$TMP/app/$f" ] || die "falta $f en el publish"
done
if find "$TMP/app" -name '*.db*' | grep -q .; then
  die "el publish contiene archivos de BD; no deben ir en la imagen"
fi

paso "3. Empaquetando app-linux.zip"
(cd "$TMP/app" && python3 -I -m zipfile -c "$TMP/app-linux.zip" .)
ls -lh "$TMP/app-linux.zip" | awk '{print "    " $5}'

paso "4. Copiando al servidor: $DESTINO"
[ -f "$DESTINO/app-linux.zip" ] && cp "$DESTINO/app-linux.zip" "$DESTINO/app-linux.prev.zip"
cp "$TMP/app-linux.zip" "$DESTINO/app-linux.zip"
cp "$REPO/deploy/Dockerfile.menusemanal" "$REPO/deploy/actualizar-menusemanal.sh" "$DESTINO/"

echo
echo "Listo: versión $VERSION copiada."
echo "En el servidor ejecuta:"
echo "  sudo bash /mnt/data/aplicaciones-web/MenuSemanal/actualizar-menusemanal.sh"

#!/bin/bash
# =====================================================================
# actualizar-menusemanal.sh — se ejecuta EN EL SERVIDOR (como root).
#   sudo bash /mnt/data/aplicaciones-web/MenuSemanal/actualizar-menusemanal.sh
#
#   1. Construye la imagen nueva desde app-linux.zip (la app sigue arriba).
#   2. Detiene el contenedor y respalda la BD (copia consistente, sin escrituras).
#   3. Arranca la versión nueva; al iniciar aplica las migraciones pendientes.
#   4. Si /health no responde: ROLLBACK automático (restaura la BD y la imagen previa).
#      Importante: tras migrar, la imagen anterior NO funciona con la BD nueva,
#      por eso el rollback siempre restaura también el backup de la BD.
# =====================================================================
set -uo pipefail

SRC=${SRC:-/mnt/data/aplicaciones-web/MenuSemanal}
ZIP="$SRC/app-linux.zip"
STAGE=${STAGE:-/opt/menusemanal-app}
DATA=${DATA:-/opt/menusemanal/data}
BACKUP_ROOT=${BACKUP_ROOT:-/root/backup-menusemanal}
CONTAINER=${CONTAINER:-menusemanal}
PORT=${PORT:-4002}
APP_TZ=${APP_TZ:-America/Bogota}
URL="http://localhost:$PORT"

die()  { echo "ERROR: $*" >&2; exit 1; }
paso() { echo; echo ">>> $*"; }

arrancar() {  # $1 = imagen
  docker run -d --name "$CONTAINER" --restart unless-stopped \
    -p "$PORT:8080" \
    -v "$DATA:/app/data" \
    -e ASPNETCORE_ENVIRONMENT=Production \
    -e DB_PATH=/app/data/menus.db \
    -e TZ="$APP_TZ" \
    "$1" >/dev/null
}

esperar_health() {  # hasta 90 s (la primera vez aplica migraciones)
  for _ in $(seq 1 45); do
    curl -fsS "$URL/health" >/dev/null 2>&1 && return 0
    sleep 2
  done
  return 1
}

contar_menus() {  # filas del historial (incluye la cabecera)
  curl -fsS "$URL/Admin/Menus/List" 2>/dev/null | grep -o '<tr>' | wc -l
}

echo "=============================================================="
echo " Actualizando MenuSemanal (Docker, puerto $PORT) - $(date '+%F %T')"
echo "=============================================================="

[ "$(id -u)" -eq 0 ] || die "ejecútalo con sudo"
for c in docker unzip curl; do command -v "$c" >/dev/null || die "falta el comando $c"; done
[ -f "$ZIP" ] || die "no existe $ZIP (ejecuta deploy/publicar.sh en tu equipo)"
[ -d "$DATA" ] || die "no existe el directorio de datos $DATA"

paso "1. Preparando contexto de build"
rm -rf "$STAGE" && mkdir -p "$STAGE"
unzip -oq "$ZIP" -d "$STAGE" || die "no se pudo descomprimir $ZIP"
cp "$SRC/Dockerfile.menusemanal" "$STAGE/Dockerfile" || die "falta $SRC/Dockerfile.menusemanal"
for f in Dockerfile MenuSemanal.dll libe_sqlite3.so Data/comidas.json VERSION; do
  [ -f "$STAGE/$f" ] && echo "    OK $f" || die "falta $f en el zip"
done
VERSION=$(tr -d '[:space:]' < "$STAGE/VERSION")
IMG="menusemanal:$VERSION"
echo "    versión: $VERSION"

paso "2. Construyendo imagen $IMG (la versión actual sigue en servicio)"
docker build -t "$IMG" "$STAGE" || die "falló el build; no se tocó nada"

paso "3. Estado actual"
PREV_IMG=$(docker inspect -f '{{.Config.Image}}' "$CONTAINER" 2>/dev/null || true)
echo "    imagen actual: ${PREV_IMG:-<ninguna>}"
[ "$PREV_IMG" = "$IMG" ] && die "esa versión ya está desplegada"
MENUS_ANTES=$(contar_menus)
echo "    filas en historial de menús: $MENUS_ANTES"

paso "4. Deteniendo el contenedor y respaldando la BD"
docker stop "$CONTAINER" >/dev/null 2>&1 || true
mkdir -p "$BACKUP_ROOT"
BK="$BACKUP_ROOT/data-$(date +%Y%m%d-%H%M%S)"
if ! cp -a "$DATA" "$BK"; then
  docker start "$CONTAINER" >/dev/null 2>&1
  die "no se pudo hacer el backup; se volvió a arrancar la versión anterior"
fi
echo "    backup: $BK"
ls -la "$BK" | sed 's/^/    /'

paso "5. Arrancando $IMG"
docker rm -f "$CONTAINER" >/dev/null 2>&1 || true
arrancar "$IMG"

if ! esperar_health; then
  echo "    *** /health NO responde. Últimas líneas del log: ***"
  docker logs --tail 60 "$CONTAINER"
  if [ -n "$PREV_IMG" ]; then
    paso "ROLLBACK automático a $PREV_IMG (restaurando la BD de $BK)"
    docker rm -f "$CONTAINER" >/dev/null 2>&1
    rm -rf "$DATA" && cp -a "$BK" "$DATA"
    arrancar "$PREV_IMG"
    esperar_health && echo "    versión anterior restaurada y respondiendo" \
                   || echo "    *** la versión anterior tampoco responde: revisar a mano ***"
  fi
  exit 1
fi

paso "6. Verificaciones"
FALLOS=0
check() {  # $1 = descripción, $2 = valor, $3 = esperado
  if [ "$2" = "$3" ]; then echo "    OK  $1: $2"; else echo "    MAL $1: $2 (esperado $3)"; FALLOS=$((FALLOS+1)); fi
}
HOME_HTML=$(curl -fsS "$URL/")
check "GET /admin"             "$(curl -s -o /dev/null -w '%{http_code}' "$URL/Admin")" 200
check "GET /Admin/Platos"      "$(curl -s -o /dev/null -w '%{http_code}' "$URL/Admin/Platos")" 200
check "navbar superior"        "$(grep -c 'navbar-brand' <<<"$HOME_HTML")" 0
check "columnas de días"       "$(grep -o 'dia-header' <<<"$HOME_HTML" | wc -l)" 7
check "casillas editables"     "$(grep -o '<select' <<<"$HOME_HTML" | wc -l)" 21
check "filas en historial"     "$(contar_menus)" "$MENUS_ANTES"
echo "    migraciones aplicadas en este arranque:"
docker logs "$CONTAINER" 2>&1 | grep -o "Applying migration '[^']*'" | sed 's/^/      /' || echo "      (ninguna)"
ERRORES=$(docker logs "$CONTAINER" 2>&1 | grep -cE '^(fail|crit):')
check "errores en el log"      "$ERRORES" 0

echo
echo "=============================================================="
if [ "$FALLOS" -eq 0 ]; then
  echo " Listo: $IMG en http://192.168.2.22:$PORT"
else
  echo " $IMG está arriba pero hay $FALLOS verificación(es) con problemas."
fi
echo
echo " Rollback manual (restaura la BD previa a la migración):"
echo "   docker rm -f $CONTAINER"
echo "   rm -rf $DATA && cp -a $BK $DATA"
echo "   docker run -d --name $CONTAINER --restart unless-stopped -p $PORT:8080 \\"
echo "     -v $DATA:/app/data -e ASPNETCORE_ENVIRONMENT=Production \\"
echo "     -e DB_PATH=/app/data/menus.db -e TZ=$APP_TZ ${PREV_IMG:-<imagen-anterior>}"
echo
echo " Imágenes de MenuSemanal en el servidor (borra las viejas con docker rmi):"
docker images menusemanal --format '   {{.Tag}}\t{{.CreatedSince}}\t{{.Size}}'
echo "=============================================================="
[ "$FALLOS" -eq 0 ]

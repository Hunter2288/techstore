#!/usr/bin/env bash
# Розгортання всіх трьох баз. Потрібні: sqlcmd, mongosh.
# Використання: SQL_SERVER=localhost SQL_USER=sa SQL_PASS='...' MONGO_URI=mongodb://localhost:27017 ./deploy.sh
set -euo pipefail
cd "$(dirname "$0")"
SQL="sqlcmd -S ${SQL_SERVER:-localhost} -U ${SQL_USER:-sa} -P ${SQL_PASS:?set SQL_PASS} -C -b -I -f 65001 -i"
MONGO="mongosh ${MONGO_URI:-mongodb://localhost:27017} --quiet --file"
$SQL p1/schema.sql; $SQL p1/procedures.sql; $SQL p1/seed.sql; $SQL p1/security.sql
$SQL p2/schema.sql; $SQL p2/seed.sql
$MONGO p3/collections.js; $MONGO p3/seed.js
echo "Готово. Перевірки: p1/checks.sql, p2/checks.sql, p3/checks.js"

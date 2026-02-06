using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace binturong.Infrastructure.Database.Postgres.Migrations
{
    public partial class PendingChanges1_20260206 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear columna nueva UUID
            migrationBuilder.Sql(
                """
                    ALTER TABLE "AuditLog"
                    ADD COLUMN entity_id_uuid uuid;
                """
            );

            // 2. NO existe conversión válida int -> uuid
            //    Así que dejamos NULL (audit logs viejos no se rompen)
            //    Si más adelante querés backfillear, se hace con lógica de negocio

            // 3. Eliminar columna vieja
            migrationBuilder.Sql(
                """
                    ALTER TABLE "AuditLog"
                    DROP COLUMN entity_id;
                """
            );

            // 4. Renombrar columna nueva
            migrationBuilder.Sql(
                """
                    ALTER TABLE "AuditLog"
                    RENAME COLUMN entity_id_uuid TO entity_id;
                """
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Crear columna int de vuelta
            migrationBuilder.Sql(
                """
                    ALTER TABLE "AuditLog"
                    ADD COLUMN entity_id_int integer;
                """
            );

            // 2. No se puede convertir uuid -> int
            //    Se deja NULL

            // 3. Drop uuid
            migrationBuilder.Sql(
                """
                    ALTER TABLE "AuditLog"
                    DROP COLUMN entity_id;
                """
            );

            // 4. Renombrar int
            migrationBuilder.Sql(
                """
                    ALTER TABLE "AuditLog"
                    RENAME COLUMN entity_id_int TO entity_id;
                """
            );
        }
    }
}

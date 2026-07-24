# Contexto - ConsultaPoliza

Ultima actualizacion: 2026-07-24.

## Objetivo

Aplicacion local para buscar una poliza por ramo, numero, certificado y fecha
de efecto, y mostrar sus datos. La solucion esta en
`C:\Users\Elisabet Galarza\Documents\API Consulta` y contiene:

- `ConsultaPoliza.Api`: ASP.NET Core Web API en C# (`net8.0`).
- `ConsultaPoliza.WinForms`: interfaz WinForms en VB.NET (`net8.0-windows`).

## Estado funcional validado

- No existe login, JWT, usuario de aplicacion ni token.
- API y WinForms usan `http://localhost:5045`.
- `GET /api/ramos` devuelve el catalogo para el desplegable.
- `GET /api/polizas?ramo=...&numeroPoliza=...&certificado=...&fechaEfecto=...`
  realiza la nueva busqueda completa.
- `GET /api/polizas/{numeroPoliza}` se mantiene por compatibilidad.
- Ramo y poliza deben ser positivos; certificado puede ser cero; fecha de
  efecto es obligatoria.
- La solucion compila con 0 errores y 0 advertencias.
- WinForms interpreta `ProblemDetails` y muestra el mensaje de error, no el JSON
  completo.
- La API devuelve `503` con un mensaje claro cuando no puede conectarse a Oracle.
- WinForms carga los ramos al abrir, impide escribir valores libres en el combo,
  usa `NumericUpDown` para certificado y `DateTimePicker` para fecha.
- El alto inicial de la ventana se adapta al contenido sin cambiar el ancho
  configurado.

Prueba real validada el 2026-07-24:

```json
{
  "numeroPoliza": "1065691",
  "numeroCertificado": 0,
  "estado": "",
  "asegurado": "RAMIREZ, YRMA YSABEL",
  "productoCodigo": 3092,
  "producto": "Seguro de Compra Protegida",
  "vigenciaDesde": "2023-01-04T00:00:00",
  "vigenciaHasta": "2024-01-04T00:00:00",
  "ramoCodigo": 7,
  "ramo": "Riesgos Varios",
  "numeroCliente": "00000010523378",
  "coberturaPrincipal": 340,
  "fechaEfecto": "2026-07-24T00:00:00"
}
```

La misma combinacion fue probada desde WinForms y mostro correctamente los
datos. `GET /api/ramos` devolvio 23 ramos.

## Oracle

La cadena de conexion esta en .NET User Secrets bajo
`OraclePolicy:ConnectionString`. No guardar la clave ni la cadena completa en
el repositorio. User Secrets ID: `consulta-poliza-api-dev`.

La VPN usa un adaptador TAP. Desde la terminal normal de la usuaria,
`Test-NetConnection coronasvimant.onfs.chi -Port 1521` devolvio `True`. Las
herramientas aisladas de Codex pueden no heredar la ruta VPN; para pruebas reales
contra Oracle puede ser necesario ejecutar con permisos fuera del aislamiento.

Restriccion permanente: no ejecutar DML ni DDL. La API inicia cada operacion con
`SET TRANSACTION READ ONLY` y finaliza con `ROLLBACK`.

El 2026-07-23 se valido la conexion configurada para la nueva base mediante
consultas de solo lectura:

- Base: `VTIMEDBP`.
- Servicio: `GALICIAVT`.
- Esquema actual: `INSUDB`.

En esta base, la tabla fisica que contiene la descripcion de las coberturas es
`INSUDB.TCOVER`: `NCOVER` identifica la cobertura y `SDESCRIPT` contiene su
descripcion. La identificacion completa tambien depende de ramo, producto,
modulo y vigencia; no consultar solamente por `NCOVER`.

Existen ademas las vistas `INSUDB.GCV_COVERAGEDESCRIPTION` y
`INSUDB.GSCOVERAGEDESCRIPTION`, que exponen `SDESCRIPT` y `SSHORT_DES` junto con
claves de cobertura. `GCV_COVERAGEDESCRIPTION` incluye ramo, producto, modulo y
cobertura; `GSCOVERAGEDESCRIPTION` agrega poliza y certificado.

La definicion de `INSUDB.GCV_COVERAGEDESCRIPTION` fue validada en modo de solo
lectura. Depende directamente de cuatro tablas:

- `GEN_COVER`, relacionada con `TAB_GENCOV` por `NCOVERGEN`.
- `LIFE_COVER`, relacionada con `TAB_LIFCOV` por `NCOVERGEN`.

La vista combina ambos resultados con `UNION`, toma `SDESCRIPT` y `SSHORT_DES`
de las tablas `TAB_GENCOV`/`TAB_LIFCOV`, y solo incluye registros de cobertura
sin fecha de baja (`DNULLDATE IS NULL`) y activos (`SSTATREGT = 1`).

## Obtencion de datos

`OraclePolicyRepository` ejecuta un `SELECT` parametrizado sobre `POLICY` y
`CERTIFICAT`. La nueva consulta filtra exactamente por `NBRANCH`, `NPOLICY` y
`NCERTIF`; no concatena entradas. La fecha elegida no se compara con
`DSTARTDATE`: se usa como fecha de corte para `REACOVER_PPAL`, porque puede ser
posterior a la vigencia de una poliza terminada.

`OracleBranchRepository` obtiene todos los ramos de `TABLE10`, ordenados por
`NBRANCH`. En la base nueva se validaron 23 registros.

Datos base obtenidos por el SELECT:

- Numero y tipo de poliza.
- Codigo de ramo y producto.
- Codigo de cliente.
- Fecha de inicio y fin.
- Tipo de modulo.

Luego `ReaGeneralPackage` llama funciones existentes de `REAGENERALPKG`:

- `REASBRANCH`: descripcion del ramo.
- `REASPRODUCT`: descripcion del producto.
- `REANAMECLI`: nombre del cliente.
- `REACOVER_PPAL`: cobertura principal.

El package se usa solo internamente para completar los datos de la poliza. No
existe ni debe agregarse un endpoint para enumerar o devolver las funciones,
procedures o argumentos de `REAGENERALPKG`.

No crear funciones, procedures, packages ni otros objetos en Oracle.

## Estado pendiente

- `estado` se devuelve vacio porque su fuente correcta aun no fue identificada.
- No volver a usar `PO.STATUS_CODE`: Oracle devolvio
  `ORA-00904: "PO"."STATUS_CODE": invalid identifier`.
- `vigenciaHasta` fue `null` para la poliza `203561`; es el valor recibido desde
  Oracle.
- Los campos adicionales de la pantalla de referencia (frecuencia de pago,
  proxima facturacion, motivos y fechas de anulacion, renovacion, entre otros)
  todavia no se incorporaron porque falta identificar y validar sus fuentes.

## Archivos principales

- `ConsultaPoliza.Api/Services/PolicyRepository.cs`: conexion y SELECT base.
- `ConsultaPoliza.Api/Services/BranchRepository.cs`: catalogo de ramos.
- `ConsultaPoliza.Api/Services/OracleReadOnlySession.cs`: transaccion read-only.
- `ConsultaPoliza.Api/Services/ReaGeneralPackage.cs`: funciones del package.
- `ConsultaPoliza.Api/Services/PolicyResponseBuilder.cs`: arma la respuesta.
- `ConsultaPoliza.Api/Controllers/PoliciesController.cs`: endpoint y errores.
- `ConsultaPoliza.Api/Controllers/BranchesController.cs`: endpoint de ramos.
- `ConsultaPoliza.Api/Options/OraclePolicyOptions.cs`: opcion de conexion.
- `ConsultaPoliza.WinForms/ApiClient.vb`: cliente HTTP y errores.
- `ConsultaPoliza.WinForms/MainForm.vb`: interfaz de consulta.
- `consulta_poliza_solo_tablas.sql`: SELECT de analisis equivalente, sin package.
- `README.md`: configuracion y comandos de ejecucion.

## Ejecucion

Desde la raiz de la solucion, en dos terminales:

```powershell
dotnet run --project .\ConsultaPoliza.Api\ConsultaPoliza.Api.csproj --launch-profile http
```

```powershell
dotnet run --project .\ConsultaPoliza.WinForms\ConsultaPoliza.WinForms.vbproj
```

Prueba directa:

```http
GET http://localhost:5045/api/ramos
GET http://localhost:5045/api/polizas?ramo=7&numeroPoliza=1065691&certificado=0&fechaEfecto=2026-07-24
```

Si una compilacion indica que `ConsultaPoliza.Api.exe` esta bloqueado, detener la
API abierta con `Ctrl+C` y volver a compilar.

# Contexto - ConsultaPoliza

Ultima actualizacion: 2026-08-04.

## Objetivo

Aplicacion local para buscar una poliza por ramo, numero, certificado y fecha
de efecto, y mostrar sus datos. La solucion esta en
`C:\Users\Elisabet Galarza\Documents\API Consulta` y contiene:

- `ConsultaPoliza.Api`: ASP.NET Web API 2 en VB.NET sobre .NET Framework 4.8 (`net48`).
- `ConsultaPoliza.WinForms`: interfaz WinForms en VB.NET (`net8.0-windows`).

## Estado funcional validado

- No existe login, JWT, usuario de aplicacion ni token.
- API y WinForms usan `http://localhost:5045`.
- `GET /api/ramos` devuelve el catalogo para el desplegable.
- `GET /api/polizas?ramo=...&numeroPoliza=...&certificado=...&fechaEfecto=...`
  realiza la nueva busqueda completa.
- Ramo y poliza deben ser positivos; certificado puede ser cero; fecha de
  efecto es obligatoria.
- La solucion compila con 0 errores y 0 advertencias.
- WinForms interpreta errores JSON con `detail` o `message` y muestra el
  mensaje de error, no el JSON completo.
- La API devuelve `503` con un mensaje claro cuando no puede conectarse a Oracle.
- WinForms carga los ramos al abrir, impide escribir valores libres en el combo,
  usa `NumericUpDown` para certificado y `DateTimePicker` para fecha.
- El alto inicial de la ventana se adapta al contenido sin cambiar el ancho
  configurado.
- WinForms muestra un menu lateral tipo arbol despues de consultar una poliza.
  El nodo `Poliza` conserva el resumen principal; `Estado`, `Roles`,
  `Intermediarios`, `Debitos directos`, `Coberturas`, `Clausulas`,
  `Descuentos / Recargos`, `Movimientos historicos`, `Recibos` y
  `Direccion de poliza` quedan navegables. Las secciones sin campos en la
  respuesta actual muestran un mensaje de datos no disponibles.
- La pestaña lateral `Estado` se carga con datos reales de Oracle. El resumen
  principal usa `CERTIFICAT.SSTATUSVA` o `POLICY.SSTATUS_POL`, descrito por
  `TABLE181`. La grilla de la pestaña usa `CERT_STATUS.NFACESTATUS`, descrito
  por `TABLE6765`; el motivo de anulacion usa `CERTIFICAT.NNULLCODE` o
  `POLICY.NNULLCODE`, descrito por `TABLE13`; la fecha efectiva de anulacion
  usa `CERTIFICAT.DNULLDATE` o `POLICY.DNULLDATE`; el motivo de suspension usa
  `CERTIFICAT.NSUS_REASON`, descrito por `TABLE5566`.
- La pestaña lateral `Roles` se carga con datos reales de Oracle. La grilla usa
  `ROLES` filtrando por `SCERTYPE`, `NBRANCH`, `NPRODUCT`, `NPOLICY` y
  `NCERTIF`; la descripcion del rol sale de `TABLE12.SDESCRIPT`; el nombre del
  cliente sale de `CLIENT.SCLIENAME`; las fechas salen de `ROLES.DNULLDATE` y
  `ROLES.DEFFECDATE`.
- La subpestaña `Direcciones` de cada rol se carga con datos reales del cliente
  asociado a ese rol. Se valido `HOLDING_PKG.GETADDRESSES`, pero la
  implementacion usa `ADDRESS` para conservar todos los campos visibles de la
  grilla, con `SRECTYPE` para tipo de direccion, `TAB_LOCAT` para localidad,
  `PROVINCE` para provincia y `TABLE66` para pais.

Prueba real validada el 2026-07-24 antes de la migracion de API a VB.NET Web
API 2:

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

La cadena de conexion se lee primero desde la variable de entorno
`ORACLE_POLICY_CONNECTION`. Como alternativas para despliegue, la API tambien
lee `OraclePolicy:ConnectionString` o `OraclePolicy.ConnectionString` desde
`appSettings`, y una cadena llamada `OraclePolicy` desde `connectionStrings`.
Para desarrollo local, si ninguna de esas opciones tiene valor, reutiliza el
User Secret `OraclePolicy:ConnectionString` con identificador
`consulta-poliza-api-dev`. No guardar la clave ni la cadena completa en el
repositorio.

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

`Oracle.ManagedDataAccess` para .NET Framework produjo un
`NullReferenceException` interno en `OpenAsync` y bloqueo en
`ExecuteNonQueryAsync`. Los endpoints conservan su contrato asincrono, pero cada
operacion Oracle se ejecuta dentro de `Task.Run` usando las APIs sincronicas
estables del proveedor. La transaccion sigue comenzando con
`SET TRANSACTION READ ONLY` y finalizando con `ROLLBACK`.

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

- No volver a usar `PO.STATUS_CODE`: Oracle devolvio
  `ORA-00904: "PO"."STATUS_CODE": invalid identifier`.
- `vigenciaHasta` fue `null` para la poliza `203561`; es el valor recibido desde
  Oracle.
- Los campos adicionales de la pantalla de referencia (frecuencia de pago,
  proxima facturacion, motivos y fechas de anulacion, renovacion, entre otros)
  todavia no se incorporaron porque falta identificar y validar sus fuentes.

## Archivos principales

- `ConsultaPoliza.Api/Global.asax.vb`: arranque ASP.NET Web API 2.
- `ConsultaPoliza.Api/App_Start/WebApiConfig.vb`: rutas y JSON camelCase.
- `ConsultaPoliza.Api/Services/PolicyRepository.vb`: conexion y SELECT base.
- `ConsultaPoliza.Api/Services/BranchRepository.vb`: catalogo de ramos.
- `ConsultaPoliza.Api/Services/OracleReadOnlySession.vb`: transaccion read-only.
- `ConsultaPoliza.Api/Services/ReaGeneralPackage.vb`: funciones del package.
- `ConsultaPoliza.Api/Services/PolicyResponseBuilder.vb`: arma la respuesta.
- `ConsultaPoliza.Api/Controllers/PoliciesController.vb`: endpoint y errores.
- `ConsultaPoliza.Api/Controllers/BranchesController.vb`: endpoint de ramos.
- `ConsultaPoliza.Api/Options/OraclePolicyOptions.vb`: opcion de conexion.
- `ConsultaPoliza.WinForms/ApiClient.vb`: cliente HTTP y errores.
- `ConsultaPoliza.WinForms/MainForm.vb`: interfaz de consulta.
- `README.md`: configuracion y comandos de ejecucion.

## Ejecucion

Desde la raiz de la solucion:

```powershell
dotnet restore .\ConsultaPoliza.slnx
dotnet build .\ConsultaPoliza.slnx
```

En una terminal, hospedar la API con IIS Express:

```powershell
$apiPath = (Resolve-Path .\ConsultaPoliza.Api).Path
& "C:\Program Files\IIS Express\iisexpress.exe" "/path:$apiPath" /port:5045
```

En otra terminal, iniciar WinForms:

```powershell
dotnet run --project .\ConsultaPoliza.WinForms\ConsultaPoliza.WinForms.vbproj
```

Prueba directa:

```http
GET http://localhost:5045/api/ramos
GET http://localhost:5045/api/polizas?ramo=7&numeroPoliza=1065691&certificado=0&fechaEfecto=2026-07-24
```

Validacion posterior a la migracion del 2026-07-24:

- `dotnet restore .\ConsultaPoliza.Api\ConsultaPoliza.Api.vbproj` completo
  correctamente.
- `dotnet build .\ConsultaPoliza.slnx --no-restore` compilo con 0 errores y 0
  advertencias.
- IIS Express hospedo la API en `http://localhost:5045`.
- `GET /api/polizas?ramo=0&numeroPoliza=abc&certificado=0&fechaEfecto=2026-07-24`
  devolvio `400` con JSON `message`.
- La API reutilizo correctamente el User Secret local de la version anterior,
  sin copiar la cadena al repositorio.
- `GET /api/ramos` devolvio `200` y 23 ramos desde Oracle.
- `GET /api/polizas?ramo=7&numeroPoliza=1065691&certificado=0&fechaEfecto=2026-07-24`
  devolvio `200` con la poliza, asegurado y producto esperados.
- WinForms se reinicio contra la API corregida; el combo cargo
  `1; Incendio` y desaparecio el error de carga de ramos.
- WinForms incorporo el `TreeView` lateral y panel derecho por seccion. La
  solucion compilo con `dotnet build .\ConsultaPoliza.slnx --no-restore` con
  0 errores y 0 advertencias.
- Se corrigio un cierre al iniciar WinForms causado por
  `System.InvalidOperationException: SplitterDistance must be between
  Panel1MinSize and Width - Panel2MinSize`. La ventana fue abierta con
  Computer Use, cargo ramos desde la API, consulto la poliza `1065691` del ramo
  `7` con certificado `0` y fecha `24/07/2026`, y navego correctamente al nodo
  `Roles`.
- Para la pestaña `Estado`, se validaron metadatos y datos en Oracle mediante
  consultas auxiliares de solo lectura. No se encontro salida
  util para la poliza de prueba en `GSCO008PKG.REAEXPIR_STATUS_GSCO009`, pero si
  se confirmaron las tablas reales. `GET /api/polizas?ramo=7&numeroPoliza=1065691&certificado=0&fechaEfecto=2026-07-24`
  devolvio `estado = "6 - Terminada"` y `estadoDetalle` con
  `estado = "4 - Anulada"`, `motivoAnulacion = "12 - Por Pedido del Asegurado"`,
  `fechaEfectivaAnulacion = "2023-07-04T00:00:00"` y `motivoSuspension = ""`.
  `dotnet build .\ConsultaPoliza.slnx --no-restore` compilo con 0 errores y 0
  advertencias.
- Para la pestaña `Roles`, se valido primero `INSREAROLESPKG.INSREAROLES`, pero
  no se uso porque devolvio clientes adicionales que no coincidian con la grilla
  de referencia. Se confirmo que la fuente correcta es `ROLES` con catalogo
  `TABLE12` y datos de cliente en `CLIENT`. `GET /api/polizas?ramo=7&numeroPoliza=1065691&certificado=0&fechaEfecto=2026-07-24`
  devolvio `roles` con `1 - Contratante` y `2 - Asegurado`, ambos para
  `00000010523378 - RAMIREZ, YRMA YSABEL`, `fechaAnulacion = null` y
  `fechaEfecto = "2021-01-04T00:00:00"`. `dotnet build .\ConsultaPoliza.slnx --no-restore`
  compilo con 0 errores y 0 advertencias.
- Para la subpestaña `Direcciones` de Roles, se valido
  `HOLDING_PKG.GETADDRESSES`, que devolvio tres filas coincidentes con la
  pantalla de referencia, y se confirmo la consulta directa sobre `ADDRESS` con
  catalogos `TAB_LOCAT`, `PROVINCE` y `TABLE66`. `GET /api/polizas?ramo=7&numeroPoliza=1065691&certificado=0&fechaEfecto=2026-07-24`
  devolvio en `roles[0].direcciones` tres registros: `1 - Comercial`,
  `2 - Particular` y `1 - Comercial`, todos con `GABOTO 1536`, codigo postal
  `3000`, localidad `17576 - PUEBLO NUEVO`, provincia `21 - Santa Fe` y pais
  `Argentina`; los dos primeros incluyen
  `PruebaSistemas@galiciaseguros.com.ar`.

## Limpieza del repositorio

El 2026-08-04 se eliminaron las herramientas temporales usadas para investigar
Oracle (`tools/OracleCoverageProbe`, `tools/OracleSearchProbe` y
`tools/OracleViewProbe`), sus salidas `bin`/`obj`, el SQL independiente de
analisis `consulta_poliza_solo_tablas.sql` y los logs de prueba de IIS Express.
Estos archivos no forman parte de la API, WinForms ni de la solucion y no son
necesarios para compilar o ejecutar el proyecto.

Tambien se retiro el flujo antiguo `GET /api/polizas/{numeroPoliza}`, junto con
`GetByNumberAsync` y su consulta Oracle. La unica consulta de polizas vigente es
`GET /api/polizas?ramo=...&numeroPoliza=...&certificado=...&fechaEfecto=...`.
La ruta convencional `api/{controller}/{id}` tambien fue eliminada; la API usa
exclusivamente las rutas por atributos declaradas en sus controladores.

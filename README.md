# ConsultaPoliza

Solucion para consultar polizas contra Oracle mediante una API ASP.NET Web API 2 en VB.NET sobre .NET Framework 4.8 y una interfaz WinForms en VB.NET.

## Proyectos

- `ConsultaPoliza.Api`: API VB.NET Web API 2 (`net48`) con consultas parametrizadas y funciones de `REAGENERALPKG`.
- `ConsultaPoliza.WinForms`: UI VB.NET con ramo, poliza, certificado y fecha de efecto.

## Configuracion de Oracle

Para desarrollo local, la API reutiliza el User Secret
`OraclePolicy:ConnectionString` identificado por `consulta-poliza-api-dev`. Se
puede configurar sin guardar la clave en el repositorio:

```powershell
dotnet user-secrets set "OraclePolicy:ConnectionString" "CADENA_DE_CONEXION" --project .\ConsultaPoliza.Api\ConsultaPoliza.Api.vbproj
```

La variable de entorno `ORACLE_POLICY_CONNECTION` tiene prioridad y es la opcion
recomendada para despliegues:

```powershell
$env:ORACLE_POLICY_CONNECTION = "CADENA_DE_CONEXION"
```

Tambien se puede configurar `OraclePolicy:ConnectionString` en `Web.config`,
`OraclePolicy.ConnectionString` en `appSettings`, o una cadena llamada
`OraclePolicy` en `connectionStrings`, pero no guardar credenciales reales en el
repositorio.

## Ejecutar

```powershell
dotnet restore .\ConsultaPoliza.slnx
dotnet build .\ConsultaPoliza.slnx
```

La API Web API 2 se hospeda con IIS Express:

```powershell
$apiPath = (Resolve-Path .\ConsultaPoliza.Api).Path
& "C:\Program Files\IIS Express\iisexpress.exe" "/path:$apiPath" /port:5045
```

En otra terminal:

```powershell
dotnet run --project .\ConsultaPoliza.WinForms\ConsultaPoliza.WinForms.vbproj
```

La UI usa por defecto `http://localhost:5045`, que coincide con el puerto de IIS Express indicado arriba.

## Probar la API

1. Inicia la API.
2. Consulta los ramos:

```http
GET http://localhost:5045/api/ramos
```

3. Consulta una poliza por todos sus criterios:

```http
GET http://localhost:5045/api/polizas?ramo=7&numeroPoliza=1065691&certificado=0&fechaEfecto=2026-07-24
```

El formato de `fechaEfecto` para la API es `AAAA-MM-DD`. En WinForms se elige
desde un calendario y se muestra como `DD/MM/AAAA`.

El desplegable obtiene los ramos desde `TABLE10`. La consulta de poliza busca
por ramo, numero y certificado en `POLICY` y `CERTIFICAT`. La fecha de efecto se
usa como fecha de corte para funciones que dependen de vigencia. Las
descripciones de ramo, producto, asegurado y cobertura principal se resuelven
mediante funciones de `REAGENERALPKG`.

## Seguridad de solo lectura

La API abre las operaciones Oracle con `SET TRANSACTION READ ONLY` y ejecuta `ROLLBACK` al terminar. Esto evita cambios directos desde las consultas de la API. Para una garantia completa, el usuario Oracle debe tener permisos solo de consulta y los packages ejecutados no deben realizar escrituras ni usar transacciones autonomas.

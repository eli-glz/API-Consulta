# ConsultaPoliza

Solucion para consultar polizas contra Oracle mediante una Web API ASP.NET Core y una interfaz WinForms en VB.NET.

## Proyectos

- `ConsultaPoliza.Api`: API C# con consultas parametrizadas y funciones de `REAGENERALPKG`.
- `ConsultaPoliza.WinForms`: UI VB.NET con ramo, poliza, certificado y fecha de efecto.

## Configuracion recomendada con User Secrets

Desde la carpeta `ConsultaPoliza.Api`:

```powershell
dotnet user-secrets set "OraclePolicy:ConnectionString" "User Id=USUARIO;Password=CLAVE;Data Source=HOST:1521/SERVICIO"
```

Ejemplo de conexion por SID:

```powershell
dotnet user-secrets set "OraclePolicy:ConnectionString" "User Id=USUARIO;Password=CLAVE;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=HOST_ORACLE)(PORT=1521))(CONNECT_DATA=(SID=SID_ORACLE)))"
```

## Ejecutar

```powershell
dotnet restore .\ConsultaPoliza.slnx
dotnet run --project .\ConsultaPoliza.Api\ConsultaPoliza.Api.csproj --launch-profile http
dotnet run --project .\ConsultaPoliza.WinForms\ConsultaPoliza.WinForms.vbproj
```

La UI usa por defecto `http://localhost:5045`, que coincide con el perfil `http` de la API.

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

El endpoint anterior `GET /api/polizas/{numeroPoliza}` se conserva por
compatibilidad.

## Seguridad de solo lectura

La API abre las operaciones Oracle con `SET TRANSACTION READ ONLY` y ejecuta `ROLLBACK` al terminar. Esto evita cambios directos desde las consultas de la API. Para una garantia completa, el usuario Oracle debe tener permisos solo de consulta y los packages ejecutados no deben realizar escrituras ni usar transacciones autonomas.

using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

var connectionString = Environment.GetEnvironmentVariable("ORACLE_POLICY_CONNECTION");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ORACLE_POLICY_CONNECTION is not configured.");
}

var mode = args.Length > 0 ? args[0] : "status";

await using var connection = new OracleConnection(connectionString);
await connection.OpenAsync();

await using (var begin = connection.CreateCommand())
{
    begin.CommandText = "SET TRANSACTION READ ONLY";
    await begin.ExecuteNonQueryAsync();
}

try
{
    switch (mode)
    {
        case "status":
            await RunStatusDiscovery(connection);
            break;
        case "status-focus":
            await RunStatusFocus(connection);
            break;
        case "status-lookups":
            await RunStatusLookups(connection);
            break;
        case "roles-discovery":
            await RunRolesDiscovery(connection);
            break;
        case "roles-focus":
            await RunRolesFocus(connection);
            break;
        case "role-addresses-discovery":
            await RunRoleAddressesDiscovery(connection);
            break;
        case "role-addresses-focus":
            await RunRoleAddressesFocus(connection);
            break;
        case "role-addresses-grid":
            await RunRoleAddressesGrid(connection);
            break;
        case "role-addresses-lookups":
            await RunRoleAddressesLookups(connection);
            break;
        default:
            throw new ArgumentException($"Unknown mode: {mode}");
    }
}
finally
{
    await using var rollback = connection.CreateCommand();
    rollback.CommandText = "ROLLBACK";
    await rollback.ExecuteNonQueryAsync();
}

static async Task RunRolesFocus(OracleConnection connection)
{
    await PrintQuery(
        connection,
        "ROLES joined with TABLE12 for policy 1065691",
        """
        SELECT R.NROLE,
               T.SDESCRIPT AS ROLE_DESCRIPTION,
               R.SCLIENT,
               R.DNULLDATE,
               R.DEFFECDATE,
               R.DCOMPDATE,
               R.NSTATUSROL
          FROM ROLES R
          LEFT JOIN TABLE12 T
            ON T.NROLE = R.NROLE
           AND T.SSTATREGT = '1'
         WHERE R.SCERTYPE = '2'
           AND R.NBRANCH = 7
           AND R.NPRODUCT = 3092
           AND R.NPOLICY = 1065691
           AND R.NCERTIF = 0
         ORDER BY R.NROLE, R.DEFFECDATE
        """);

    await PrintQuery(
        connection,
        "CLIENT columns",
        """
        SELECT column_id, column_name, data_type
          FROM all_tab_columns
         WHERE owner = 'INSUDB'
           AND table_name = 'CLIENT'
         ORDER BY column_id
         FETCH FIRST 120 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "CLIENT rows for role clients",
        """
        SELECT *
          FROM CLIENT
         WHERE SCLIENT IN (
             SELECT SCLIENT
               FROM ROLES
              WHERE SCERTYPE = '2'
                AND NBRANCH = 7
                AND NPRODUCT = 3092
                AND NPOLICY = 1065691
                AND NCERTIF = 0
         )
        """);

    await PrintRolesProcedure(connection, role: 1);
    await PrintRolesProcedure(connection, role: 2);
}

static async Task RunRoleAddressesDiscovery(OracleConnection connection)
{
    await PrintQuery(
        connection,
        "Candidate address package functions/procedures",
        """
        SELECT owner, package_name, object_name, overload, argument_name, position, data_type, in_out
          FROM all_arguments
         WHERE owner = 'INSUDB'
           AND package_name IS NOT NULL
           AND (
                UPPER(package_name) LIKE '%ADDR%'
             OR UPPER(package_name) LIKE '%DIREC%'
             OR UPPER(package_name) LIKE '%ADDRESS%'
             OR UPPER(package_name) LIKE '%DOMIC%'
             OR UPPER(object_name) LIKE '%ADDR%'
             OR UPPER(object_name) LIKE '%DIREC%'
             OR UPPER(object_name) LIKE '%ADDRESS%'
             OR UPPER(object_name) LIKE '%DOMIC%'
             OR UPPER(object_name) LIKE '%PHONE%'
             OR UPPER(object_name) LIKE '%EMAIL%'
           )
         ORDER BY package_name, object_name, overload, position
         FETCH FIRST 300 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "Candidate address tables/views by name",
        """
        SELECT owner, object_name, object_type
          FROM all_objects
         WHERE owner = 'INSUDB'
           AND object_type IN ('TABLE', 'VIEW')
           AND (
                UPPER(object_name) LIKE '%ADDR%'
             OR UPPER(object_name) LIKE '%DIREC%'
             OR UPPER(object_name) LIKE '%ADDRESS%'
             OR UPPER(object_name) LIKE '%DOMIC%'
             OR UPPER(object_name) LIKE '%PHONE%'
             OR UPPER(object_name) LIKE '%EMAIL%'
             OR UPPER(object_name) LIKE '%MAIL%'
             OR UPPER(object_name) LIKE '%LOCAL%'
             OR UPPER(object_name) LIKE '%CITY%'
           )
         ORDER BY object_type, object_name
         FETCH FIRST 300 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "Tables/views with SCLIENT and address-like columns",
        """
        SELECT table_name, column_id, column_name, data_type
          FROM all_tab_columns
         WHERE owner = 'INSUDB'
           AND table_name IN (
                SELECT table_name
                  FROM all_tab_columns
                 WHERE owner = 'INSUDB'
                   AND column_name = 'SCLIENT'
           )
           AND (
                column_name IN ('SCLIENT', 'SADDRESS', 'SSTREET', 'SZIP_CODE', 'SZIPCODE', 'NZIP_CODE', 'NLOCAL', 'NPROVINCE', 'NCOUNTRY', 'SE_MAIL', 'SEMAIL')
             OR UPPER(column_name) LIKE '%ADDR%'
             OR UPPER(column_name) LIKE '%DIREC%'
             OR UPPER(column_name) LIKE '%ADDRESS%'
             OR UPPER(column_name) LIKE '%STREET%'
             OR UPPER(column_name) LIKE '%ZIP%'
             OR UPPER(column_name) LIKE '%POST%'
             OR UPPER(column_name) LIKE '%LOCAL%'
             OR UPPER(column_name) LIKE '%CITY%'
             OR UPPER(column_name) LIKE '%PROVIN%'
             OR UPPER(column_name) LIKE '%COUNTR%'
             OR UPPER(column_name) LIKE '%MAIL%'
             OR UPPER(column_name) LIKE '%PHONE%'
           )
         ORDER BY table_name, column_id
         FETCH FIRST 500 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "Dictionary comments for address-like columns",
        """
        SELECT table_name, column_name, comments
          FROM all_col_comments
         WHERE owner = 'INSUDB'
           AND (
                UPPER(comments) LIKE '%DIREC%'
             OR UPPER(comments) LIKE '%ADDRESS%'
             OR UPPER(comments) LIKE '%DOMIC%'
             OR UPPER(comments) LIKE '%LOCAL%'
             OR UPPER(comments) LIKE '%PROVIN%'
             OR UPPER(comments) LIKE '%POSTAL%'
             OR UPPER(comments) LIKE '%MAIL%'
           )
         ORDER BY table_name, column_name
         FETCH FIRST 300 ROWS ONLY
        """);

    await PrintKnownAddressTables(connection);
}

static async Task RunRoleAddressesFocus(OracleConnection connection)
{
    var tables = new[]
    {
        "ADDRESS",
        "ADDRESSES",
        "CLIENT_ADDRESS",
        "CLIENT_ADDRESSES",
        "CLI_ADDRESS",
        "CLIEN_ADDRESS",
        "CLIENTADDR",
        "CLIENTS_ADDRESS",
        "CLIDIREC",
        "DIRECCION",
        "DIRECCIONES",
        "DIR_CLIENT",
        "CLIENT",
        "TABLE15",
        "TABLE16",
        "TABLE17",
        "TABLE18",
        "TABLE19",
        "TABLE62"
    };

    foreach (var tableName in tables)
    {
        await PrintQueryIfExists(
            connection,
            tableName + " columns",
            $"""
            SELECT column_id, column_name, data_type
              FROM all_tab_columns
             WHERE owner = 'INSUDB'
               AND table_name = '{tableName}'
             ORDER BY column_id
            """);

        await PrintQueryIfExists(
            connection,
            tableName + " rows for client 00000010523378",
            $"""
            SELECT *
              FROM {tableName}
             WHERE SCLIENT = '00000010523378'
                OR ROWNUM <= 5
            """);
    }
}

static async Task RunRoleAddressesGrid(OracleConnection connection)
{
    await PrintQuery(
        connection,
        "ADDRESS columns",
        """
        SELECT column_id, column_name, data_type
          FROM all_tab_columns
         WHERE owner = 'INSUDB'
           AND table_name = 'ADDRESS'
         ORDER BY column_id
        """);

    await PrintQuery(
        connection,
        "ADDRESS comments",
        """
        SELECT column_name, comments
          FROM all_col_comments
         WHERE owner = 'INSUDB'
           AND table_name = 'ADDRESS'
         ORDER BY column_name
        """);

    await PrintQuery(
        connection,
        "ADDRESS rows for role client 00000010523378",
        """
        SELECT NRECOWNER,
               SKEYADDRESS,
               DEFFECDATE,
               NTYPEOFADDRESS,
               SSTREET,
               SSTREET1,
               SBUILD,
               NFLOOR,
               SDEPARTMENT,
               SPOPULATION,
               SZIP_CODE,
               NZIP_CODE,
               NCOUNTRY,
               NPROVINCE,
               NLOCAL,
               SE_MAIL,
               SINFOR
          FROM ADDRESS
         WHERE SCLIENT = '00000010523378'
         ORDER BY NTYPEOFADDRESS, DEFFECDATE DESC
        """);

    await PrintQuery(
        connection,
        "HOLDING_PKG.GETADDRESSES arguments",
        """
        SELECT argument_name, position, data_type, in_out
          FROM all_arguments
         WHERE owner = 'INSUDB'
           AND package_name = 'HOLDING_PKG'
           AND object_name = 'GETADDRESSES'
         ORDER BY position
        """);

    await PrintHoldingAddressesProcedure(connection);

    await PrintQuery(
        connection,
        "Catalog tables with NTYPEOFADDRESS",
        """
        SELECT table_name, column_id, column_name, data_type
          FROM all_tab_columns
         WHERE owner = 'INSUDB'
           AND table_name IN (
                SELECT table_name
                  FROM all_tab_columns
                 WHERE owner = 'INSUDB'
                   AND column_name = 'NTYPEOFADDRESS'
           )
           AND (column_name IN ('NTYPEOFADDRESS', 'SDESCRIPT', 'SSHORT_DES', 'SSTATREGT') OR UPPER(column_name) LIKE '%TYPE%')
         ORDER BY table_name, column_id
        """);

    await PrintQuery(
        connection,
        "TAB_TYPADDRESS sample",
        """
        SELECT *
          FROM TAB_TYPADDRESS
         WHERE NTYPEOFADDRESS IN (
             SELECT NTYPEOFADDRESS
               FROM ADDRESS
              WHERE SCLIENT = '00000010523378'
         )
            OR ROWNUM <= 20
        """);

    await PrintQuery(
        connection,
        "Catalog candidates for country/province/local columns",
        """
        SELECT table_name,
               SUM(CASE WHEN column_name = 'NCOUNTRY' THEN 1 ELSE 0 END) AS HAS_NCOUNTRY,
               SUM(CASE WHEN column_name = 'NPROVINCE' THEN 1 ELSE 0 END) AS HAS_NPROVINCE,
               SUM(CASE WHEN column_name = 'NLOCAL' THEN 1 ELSE 0 END) AS HAS_NLOCAL,
               SUM(CASE WHEN column_name = 'SDESCRIPT' THEN 1 ELSE 0 END) AS HAS_SDESCRIPT,
               SUM(CASE WHEN column_name = 'SSTATREGT' THEN 1 ELSE 0 END) AS HAS_SSTATREGT
          FROM all_tab_columns
         WHERE owner = 'INSUDB'
         GROUP BY table_name
        HAVING SUM(CASE WHEN column_name IN ('NCOUNTRY', 'NPROVINCE', 'NLOCAL') THEN 1 ELSE 0 END) > 0
           AND SUM(CASE WHEN column_name = 'SDESCRIPT' THEN 1 ELSE 0 END) > 0
         ORDER BY table_name
         FETCH FIRST 200 ROWS ONLY
        """);

    await PrintGeoTableSamples(connection);
}

static async Task PrintHoldingAddressesProcedure(OracleConnection connection)
{
    Console.WriteLine();
    Console.WriteLine("== HOLDING_PKG.GETADDRESSES result ==");

    await using var command = connection.CreateCommand();
    command.CommandText = "HOLDING_PKG.GETADDRESSES";
    command.CommandType = CommandType.StoredProcedure;
    command.BindByName = true;
    command.Parameters.Add("CERTYPE", OracleDbType.Varchar2, "2", ParameterDirection.Input);
    command.Parameters.Add("CLIENTID", OracleDbType.Varchar2, "00000010523378", ParameterDirection.Input);
    command.Parameters.Add("BRANCHID", OracleDbType.Decimal, 7, ParameterDirection.Input);
    command.Parameters.Add("PRODUCTID", OracleDbType.Decimal, 3092, ParameterDirection.Input);
    command.Parameters.Add("POLICYID", OracleDbType.Decimal, 1065691, ParameterDirection.Input);
    command.Parameters.Add("CERTIFICATEID", OracleDbType.Decimal, 0, ParameterDirection.Input);
    command.Parameters.Add("EFFECDATE", OracleDbType.Date, new DateTime(2026, 7, 24), ParameterDirection.Input);
    command.Parameters.Add("RC1", OracleDbType.RefCursor, ParameterDirection.Output);

    try
    {
        await command.ExecuteNonQueryAsync();
        await using var reader = ((OracleRefCursor)command.Parameters["RC1"].Value).GetDataReader();
        PrintReader(reader);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
    }
}

static async Task PrintGeoTableSamples(OracleConnection connection)
{
    var candidates = new[]
    {
        "COUNTRY",
        "PROVINCE",
        "LOCALITY",
        "LOCAL",
        "ZIP_CODE",
        "TABLE11",
        "TABLE21",
        "TABLE22",
        "TABLE23",
        "TABLE24",
        "TABLE25",
        "TABLE35",
        "TABLE36",
        "TABLE37",
        "TABLE38",
        "TABLE39"
    };

    foreach (var tableName in candidates)
    {
        await PrintQueryIfExists(
            connection,
            tableName + " columns",
            $"""
            SELECT column_id, column_name, data_type
              FROM all_tab_columns
             WHERE owner = 'INSUDB'
               AND table_name = '{tableName}'
             ORDER BY column_id
            """);

        await PrintQueryIfExists(
            connection,
            tableName + " sample",
            $"""
            SELECT *
              FROM {tableName}
             WHERE ROWNUM <= 20
            """);
    }
}

static async Task RunRoleAddressesLookups(OracleConnection connection)
{
    await PrintQuery(
        connection,
        "ADDRESS direct grid candidate",
        """
        SELECT A.NRECOWNER,
               A.SRECTYPE,
               CASE A.SRECTYPE
                   WHEN '1' THEN 'Comercial'
                   WHEN '2' THEN 'Particular'
                   WHEN '3' THEN 'Casilla de correo'
                   ELSE NULL
               END AS ADDRESS_TYPE,
               A.SSTREET,
               A.SZIP_CODE,
               A.NZIP_CODE,
               A.NCOUNTRY,
               A.NPROVINCE,
               P.SDESCRIPT AS PROVINCE_DESCRIPTION,
               A.NLOCAL,
               L.SDESCRIPT AS LOCAL_DESCRIPTION,
               A.SE_MAIL,
               A.DEFFECDATE,
               A.DNULLDATE
          FROM ADDRESS A
          LEFT JOIN PROVINCE P
            ON P.NCOUNTRY = A.NCOUNTRY
           AND P.NPROVINCE = A.NPROVINCE
          LEFT JOIN TAB_LOCAT L
            ON L.NCOUNTRY = A.NCOUNTRY
           AND L.NPROVINCE = A.NPROVINCE
           AND L.NLOCAL = A.NLOCAL
         WHERE A.SCLIENT = '00000010523378'
         ORDER BY A.NRECOWNER, A.SRECTYPE
        """);

    await PrintQuery(
        connection,
        "TAB_LOCAT row",
        """
        SELECT *
          FROM TAB_LOCAT
         WHERE NCOUNTRY = 54
           AND NPROVINCE = 21
           AND NLOCAL = 17576
        """);

    await PrintQuery(
        connection,
        "PROVINCE row",
        """
        SELECT *
          FROM PROVINCE
         WHERE NCOUNTRY = 54
           AND NPROVINCE = 21
        """);

    await PrintQuery(
        connection,
        "TABLE66 columns",
        """
        SELECT column_id, column_name, data_type
          FROM all_tab_columns
         WHERE owner = 'INSUDB'
           AND table_name = 'TABLE66'
         ORDER BY column_id
        """);

    await PrintQuery(
        connection,
        "TABLE66 row for country 54",
        """
        SELECT *
          FROM TABLE66
         WHERE NCOUNTRY = 54
        """);
}

static async Task PrintKnownAddressTables(OracleConnection connection)
{
    var candidates = new[]
    {
        "ADDRESS",
        "ADDRESSES",
        "CLIENT_ADDRESS",
        "CLIENT_ADDRESSES",
        "CLI_ADDRESS",
        "CLIEN_ADDRESS",
        "CLIENTADDR",
        "CLIENTS_ADDRESS",
        "CLIDIREC",
        "DIRECCION",
        "DIRECCIONES",
        "DIR_CLIENT",
        "CLIENT",
        "TABLE15",
        "TABLE16",
        "TABLE17",
        "TABLE18",
        "TABLE19",
        "TABLE62"
    };

    foreach (var tableName in candidates)
    {
        await PrintQueryIfExists(
            connection,
            tableName,
            $"""
            SELECT *
              FROM {tableName}
             WHERE ROWNUM <= 20
            """);
    }
}

static async Task PrintRolesProcedure(OracleConnection connection, int role)
{
    Console.WriteLine();
    Console.WriteLine($"== INSREAROLESPKG.INSREAROLES result with NROLE={role} ==");

    await using var command = connection.CreateCommand();
    command.CommandText = "INSREAROLESPKG.INSREAROLES";
    command.CommandType = CommandType.StoredProcedure;
    command.BindByName = true;
    command.Parameters.Add("SCERTYPE", OracleDbType.Varchar2, "2", ParameterDirection.Input);
    command.Parameters.Add("NBRANCH", OracleDbType.Decimal, 7, ParameterDirection.Input);
    command.Parameters.Add("NPRODUCT", OracleDbType.Decimal, 3092, ParameterDirection.Input);
    command.Parameters.Add("NPOLICY", OracleDbType.Decimal, 1065691, ParameterDirection.Input);
    command.Parameters.Add("NCERTIF", OracleDbType.Decimal, 0, ParameterDirection.Input);
    command.Parameters.Add("NROLE", OracleDbType.Decimal, role, ParameterDirection.Input);
    command.Parameters.Add("DEFFECDATE", OracleDbType.Date, new DateTime(2026, 7, 24), ParameterDirection.Input);
    command.Parameters.Add("SNUMERATOR", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input);
    command.Parameters.Add("RC1", OracleDbType.RefCursor, ParameterDirection.InputOutput);

    try
    {
        await command.ExecuteNonQueryAsync();
        await using var reader = ((OracleRefCursor)command.Parameters["RC1"].Value).GetDataReader();
        PrintReader(reader);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
    }
}

static async Task RunRolesDiscovery(OracleConnection connection)
{
    await PrintQuery(
        connection,
        "Candidate role package functions/procedures",
        """
        SELECT owner, package_name, object_name, overload, argument_name, position, data_type, in_out
          FROM all_arguments
         WHERE owner = 'INSUDB'
           AND package_name IS NOT NULL
           AND (
                UPPER(package_name) LIKE '%ROLE%'
             OR UPPER(package_name) LIKE '%ROL%'
             OR UPPER(object_name) LIKE '%ROLE%'
             OR UPPER(object_name) LIKE '%ROL%'
             OR UPPER(object_name) LIKE '%CLIENT%'
             OR UPPER(object_name) LIKE '%ASEG%'
             OR UPPER(object_name) LIKE '%CONTR%'
           )
         ORDER BY package_name, object_name, overload, position
         FETCH FIRST 250 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "Candidate role tables/views by name",
        """
        SELECT owner, object_name, object_type
          FROM all_objects
         WHERE owner = 'INSUDB'
           AND object_type IN ('TABLE', 'VIEW')
           AND (
                UPPER(object_name) LIKE '%ROLE%'
             OR UPPER(object_name) LIKE '%ROL%'
             OR UPPER(object_name) LIKE '%CLIENT%'
             OR UPPER(object_name) LIKE '%CLIEN%'
             OR UPPER(object_name) LIKE '%ASEG%'
             OR UPPER(object_name) LIKE '%INSURED%'
             OR UPPER(object_name) LIKE '%HOLDER%'
             OR UPPER(object_name) LIKE '%TITULAR%'
           )
         ORDER BY object_type, object_name
         FETCH FIRST 250 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "Tables/views containing NROLE",
        """
        SELECT table_name, column_id, column_name, data_type
          FROM all_tab_columns
         WHERE owner = 'INSUDB'
           AND table_name IN (
                SELECT table_name
                  FROM all_tab_columns
                 WHERE owner = 'INSUDB'
                   AND column_name = 'NROLE'
           )
           AND (
                column_name IN ('SCERTYPE', 'NBRANCH', 'NPRODUCT', 'NPOLICY', 'NCERTIF', 'SCLIENT', 'NROLE', 'DNULLDATE', 'DSTARTDATE', 'DEFFECDATE')
             OR UPPER(column_name) LIKE '%ROLE%'
             OR UPPER(column_name) LIKE '%DATE%'
             OR UPPER(column_name) LIKE '%CLIENT%'
           )
         ORDER BY table_name, column_id
         FETCH FIRST 300 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "Role dictionary comments",
        """
        SELECT table_name, column_name, comments
          FROM all_col_comments
         WHERE owner = 'INSUDB'
           AND (
                column_name = 'NROLE'
             OR UPPER(comments) LIKE '%ROLE%'
             OR UPPER(comments) LIKE '%ROL%'
             OR UPPER(comments) LIKE '%CONTRAT%'
             OR UPPER(comments) LIKE '%ASEG%'
           )
         ORDER BY table_name, column_name
         FETCH FIRST 200 ROWS ONLY
        """);

    await PrintKnownRoleTables(connection);
}

static async Task PrintKnownRoleTables(OracleConnection connection)
{
    var candidates = new[]
    {
        "ROLES",
        "ROLE",
        "ROLECLI",
        "CLIENT_ROLES",
        "CERTIF_ROLE",
        "CERTIFICAT_ROLE",
        "POLICY_ROLE",
        "LIFE_ROLES",
        "TAB_ROLES",
        "TABLE12"
    };

    foreach (var tableName in candidates)
    {
        await PrintQueryIfExists(
            connection,
            tableName,
            $"""
            SELECT *
              FROM {tableName}
             WHERE ROWNUM <= 20
            """);
    }
}

static async Task PrintQueryIfExists(OracleConnection connection, string title, string sql)
{
    try
    {
        await PrintQuery(connection, title, sql);
    }
    catch (OracleException ex) when (ex.Number == 942 || ex.Number == 904)
    {
        Console.WriteLine();
        Console.WriteLine("== " + title + " ==");
        Console.WriteLine(ex.Message);
    }
}

static async Task RunStatusFocus(OracleConnection connection)
{
    await PrintQuery(
        connection,
        "GSCO008PKG.REAEXPIR_STATUS_GSCO009 arguments",
        """
        SELECT owner, package_name, object_name, argument_name, position, data_type, in_out
          FROM all_arguments
         WHERE owner = 'INSUDB'
           AND package_name = 'GSCO008PKG'
           AND object_name = 'REAEXPIR_STATUS_GSCO009'
         ORDER BY position
        """);

    await PrintQuery(
        connection,
        "GSCO008PKG source lines mentioning REAEXPIR_STATUS_GSCO009/status tables",
        """
        SELECT type, line, text
          FROM all_source
         WHERE owner = 'INSUDB'
           AND name = 'GSCO008PKG'
           AND (
                UPPER(text) LIKE '%REAEXPIR_STATUS_GSCO009%'
             OR UPPER(text) LIKE '%EXPIR_STATUS%'
             OR UPPER(text) LIKE '%SSTATUS_POL%'
             OR UPPER(text) LIKE '%NNULLCODE%'
           )
         ORDER BY type, line
         FETCH FIRST 120 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "Focused status table columns",
        """
        SELECT table_name, column_id, column_name, data_type
          FROM all_tab_columns
         WHERE owner = 'INSUDB'
           AND table_name IN ('POLICY', 'CERTIFICAT', 'EXPIR_STATUS', 'CERT_STATUS', 'CERT_STATUS_HIS', 'ANUL_POL', 'GS_MOTIVOSANULACION')
         ORDER BY table_name, column_id
        """);

    await PrintQuery(
        connection,
        "Policy 1065691 raw status fields",
        """
        SELECT PO.NBRANCH,
               PO.NPOLICY,
               PO.SCERTYPE,
               CE.NCERTIF,
               CE.NPRODUCT,
               CE.SCLIENT,
               CE.DSTARTDATE,
               CE.DEXPIRDAT,
               PO.SSTATUS_POL,
               PO.SNONULL,
               PO.DNULLDATE,
               PO.NNULLCODE,
               CE.SSTATUSVA,
               CE.NNULLCODE AS CE_NNULLCODE,
               CE.DNULLDATE AS CE_DNULLDATE,
               CE.NSTATUSCOVERAGECERTIFICATE
          FROM POLICY PO
          JOIN CERTIFICAT CE
            ON CE.NBRANCH = PO.NBRANCH
           AND CE.NPOLICY = PO.NPOLICY
           AND CE.SCERTYPE = PO.SCERTYPE
           AND CE.SCLIENT = PO.SCLIENT
         WHERE PO.NBRANCH = 7
           AND PO.NPOLICY = 1065691
           AND CE.NCERTIF = 0
        """);

    await PrintQuery(
        connection,
        "EXPIR_STATUS sample",
        """
        SELECT *
          FROM EXPIR_STATUS
         FETCH FIRST 30 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "CERT_STATUS sample",
        """
        SELECT *
          FROM CERT_STATUS
         FETCH FIRST 30 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "ANUL_POL sample matching policy",
        """
        SELECT *
          FROM ANUL_POL
         WHERE NBRANCH = 7
           AND NPOLICY = 1065691
         FETCH FIRST 30 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "CERT_STATUS matching policy",
        """
        SELECT *
          FROM CERT_STATUS
         WHERE SCERTYPE = '2'
           AND NBRANCH = 7
           AND NPRODUCT = 3092
           AND NPOLICY = 1065691
           AND NCERTIF = 0
        """);

    await PrintQuery(
        connection,
        "Tables containing SSTATUS_POL, SSTATUSVA, NFACESTATUS or NNULLCODE",
        """
        SELECT table_name, column_name, data_type
          FROM all_tab_columns
         WHERE owner = 'INSUDB'
           AND column_name IN ('SSTATUS_POL', 'SSTATUSVA', 'NFACESTATUS', 'NNULLCODE')
         ORDER BY column_name, table_name
         FETCH FIRST 300 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "Tables likely describing policy/certificate/null statuses",
        """
        SELECT owner, object_name, object_type
          FROM all_objects
         WHERE owner = 'INSUDB'
           AND object_type IN ('TABLE', 'VIEW')
           AND (
                UPPER(object_name) LIKE '%NULL%'
             OR UPPER(object_name) LIKE '%NUL%'
             OR UPPER(object_name) LIKE '%FACESTATUS%'
             OR UPPER(object_name) LIKE '%FACE_STATUS%'
             OR UPPER(object_name) LIKE '%POL%STAT%'
             OR UPPER(object_name) LIKE '%CERT%STAT%'
             OR UPPER(object_name) LIKE '%STATUS%CERT%'
             OR UPPER(object_name) LIKE '%STATUS%POL%'
           )
         ORDER BY object_type, object_name
         FETCH FIRST 300 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "Dictionary comments for status columns",
        """
        SELECT table_name, column_name, comments
          FROM all_col_comments
         WHERE owner = 'INSUDB'
           AND table_name IN ('POLICY', 'CERTIFICAT', 'CERT_STATUS')
           AND column_name IN ('SSTATUS_POL', 'SSTATUSVA', 'NFACESTATUS', 'NNULLCODE', 'DNULLDATE')
         ORDER BY table_name, column_name
        """);

    await PrintStatusProcedure(connection, managingId: 0);
    await PrintStatusProcedure(connection, managingId: 1);
}

static async Task RunStatusLookups(OracleConnection connection)
{
    await PrintQuery(
        connection,
        "TABLE6765 columns/sample for NFACESTATUS",
        """
        SELECT *
          FROM TABLE6765
         WHERE NFACESTATUS = 4
            OR ROWNUM <= 20
        """);

    await PrintQuery(
        connection,
        "TABLE181 columns/sample for SSTATUSVA",
        """
        SELECT *
          FROM TABLE181
         WHERE SSTATUSVA = '6'
            OR ROWNUM <= 20
        """);

    await PrintQuery(
        connection,
        "TABLE13 columns/sample for NNULLCODE",
        """
        SELECT *
          FROM TABLE13
         WHERE NNULLCODE = 12
            OR ROWNUM <= 20
        """);

    await PrintQuery(
        connection,
        "CAUSESANNUL columns/sample for NNULLCODE",
        """
        SELECT *
          FROM CAUSESANNUL
         WHERE NNULLCODE = 12
            OR ROWNUM <= 20
        """);

    await PrintQuery(
        connection,
        "NULL_CONDI columns/sample for NNULLCODE",
        """
        SELECT *
          FROM NULL_CONDI
         WHERE NNULLCODE = 12
            OR ROWNUM <= 20
        """);

    await PrintQuery(
        connection,
        "RNULLCONDI columns/sample for NNULLCODE",
        """
        SELECT *
          FROM RNULLCONDI
         WHERE NNULLCODE = 12
            OR ROWNUM <= 20
        """);

    await PrintQuery(
        connection,
        "GS_MOTIVOSANULACION sample for ID_MOTIVO",
        """
        SELECT *
          FROM GS_MOTIVOSANULACION
         WHERE ID_MOTIVO = 12
            OR ROWNUM <= 20
        """);

    await PrintQuery(
        connection,
        "VPOLICYQUERY_CERTYPE2 matching policy",
        """
        SELECT *
          FROM VPOLICYQUERY_CERTYPE2
         WHERE NBRANCH = 7
           AND NPOLICY = 1065691
           AND NCERTIF = 0
        """);

    await PrintQuery(
        connection,
        "Suspension reason candidates",
        """
        SELECT table_name, column_name, data_type
          FROM all_tab_columns
         WHERE owner = 'INSUDB'
           AND (
                column_name = 'NSUS_REASON'
             OR UPPER(table_name) LIKE '%SUSP%'
             OR UPPER(table_name) LIKE '%SUS%'
           )
         ORDER BY table_name, column_id
         FETCH FIRST 200 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "CERTIFICAT suspension fields for policy",
        """
        SELECT NSUS_REASON,
               DCOLLSUS_INI,
               DCOLLSUS_END,
               SSUS_ORIGI,
               NSUS_BRANCH,
               NSUS_PRODUCT,
               NSUS_POLICY,
               NSUS_CERTIF
          FROM CERTIFICAT
         WHERE NBRANCH = 7
           AND NPRODUCT = 3092
           AND NPOLICY = 1065691
           AND NCERTIF = 0
        """);

    await PrintQuery(
        connection,
        "TABLE5566 sample for NSUS_REASON",
        """
        SELECT *
          FROM TABLE5566
         WHERE ROWNUM <= 30
        """);
}

static async Task PrintStatusProcedure(OracleConnection connection, int managingId)
{
    Console.WriteLine();
    Console.WriteLine($"== GSCO008PKG.REAEXPIR_STATUS_GSCO009 result with NMANAGING_ID={managingId} ==");

    await using var command = connection.CreateCommand();
    command.CommandText = "GSCO008PKG.REAEXPIR_STATUS_GSCO009";
    command.CommandType = CommandType.StoredProcedure;
    command.BindByName = true;
    command.Parameters.Add("NBRANCH", OracleDbType.Decimal, 7, ParameterDirection.Input);
    command.Parameters.Add("NPRODUCT", OracleDbType.Decimal, 3092, ParameterDirection.Input);
    command.Parameters.Add("NPOLICY", OracleDbType.Decimal, 1065691, ParameterDirection.Input);
    command.Parameters.Add("NCERTIF", OracleDbType.Decimal, 0, ParameterDirection.Input);
    command.Parameters.Add("NMANAGING_ID", OracleDbType.Decimal, managingId, ParameterDirection.Input);
    command.Parameters.Add("RC1", OracleDbType.RefCursor, ParameterDirection.Output);

    try
    {
        await command.ExecuteNonQueryAsync();
        await using var reader = ((OracleRefCursor)command.Parameters["RC1"].Value).GetDataReader();
        PrintReader(reader);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
    }
}

static async Task RunStatusDiscovery(OracleConnection connection)
{
    await PrintQuery(
        connection,
        "Candidate package functions/procedures",
        """
        SELECT owner, package_name, object_name, overload, argument_name, position, data_type, in_out
          FROM all_arguments
         WHERE owner = 'INSUDB'
           AND package_name IS NOT NULL
           AND (
                UPPER(package_name) LIKE '%REA%'
             OR UPPER(object_name) LIKE '%STAT%'
             OR UPPER(object_name) LIKE '%EST%'
             OR UPPER(object_name) LIKE '%ANUL%'
             OR UPPER(object_name) LIKE '%SUSP%'
             OR UPPER(object_name) LIKE '%POL%'
           )
           AND (
                UPPER(object_name) LIKE '%STAT%'
             OR UPPER(object_name) LIKE '%EST%'
             OR UPPER(object_name) LIKE '%ANUL%'
             OR UPPER(object_name) LIKE '%SUSP%'
             OR UPPER(object_name) LIKE '%POL%'
           )
         ORDER BY package_name, object_name, overload, position
         FETCH FIRST 200 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "Candidate tables/views by name",
        """
        SELECT owner, object_name, object_type
          FROM all_objects
         WHERE owner = 'INSUDB'
           AND object_type IN ('TABLE', 'VIEW')
           AND (
                UPPER(object_name) LIKE '%STATUS%'
             OR UPPER(object_name) LIKE '%STATE%'
             OR UPPER(object_name) LIKE '%STAT%'
             OR UPPER(object_name) LIKE '%EST%'
             OR UPPER(object_name) LIKE '%ANUL%'
             OR UPPER(object_name) LIKE '%SUSP%'
             OR UPPER(object_name) LIKE '%POLICY%'
             OR UPPER(object_name) LIKE '%CERTIF%'
           )
         ORDER BY object_type, object_name
         FETCH FIRST 200 ROWS ONLY
        """);

    await PrintQuery(
        connection,
        "POLICY/CERTIFICAT status-like columns",
        """
        SELECT owner, table_name, column_name, data_type
          FROM all_tab_columns
         WHERE owner = 'INSUDB'
           AND table_name IN ('POLICY', 'CERTIFICAT')
           AND (
                UPPER(column_name) LIKE '%STAT%'
             OR UPPER(column_name) LIKE '%EST%'
             OR UPPER(column_name) LIKE '%ANUL%'
             OR UPPER(column_name) LIKE '%SUSP%'
             OR UPPER(column_name) LIKE '%NULL%'
             OR UPPER(column_name) LIKE '%CANCEL%'
             OR UPPER(column_name) LIKE '%MOT%'
             OR UPPER(column_name) LIKE '%CAUSE%'
             OR UPPER(column_name) LIKE '%DATE%'
           )
         ORDER BY table_name, column_id
        """);

    await PrintQuery(
        connection,
        "Policy 1065691 candidate raw values",
        """
        SELECT PO.NBRANCH,
               PO.NPOLICY,
               PO.SCERTYPE,
               CE.NCERTIF,
               CE.NPRODUCT,
               CE.SCLIENT,
               CE.DSTARTDATE,
               CE.DEXPIRDAT,
               PO.SSTATREGT AS PO_SSTATREGT,
               CE.SSTATREGT AS CE_SSTATREGT,
               PO.DNULLDATE AS PO_DNULLDATE,
               CE.DNULLDATE AS CE_DNULLDATE
          FROM POLICY PO
          JOIN CERTIFICAT CE
            ON CE.NBRANCH = PO.NBRANCH
           AND CE.NPOLICY = PO.NPOLICY
           AND CE.SCERTYPE = PO.SCERTYPE
           AND CE.SCLIENT = PO.SCLIENT
         WHERE PO.NBRANCH = 7
           AND PO.NPOLICY = 1065691
           AND CE.NCERTIF = 0
        """);
}

static async Task PrintQuery(OracleConnection connection, string title, string sql)
{
    Console.WriteLine();
    Console.WriteLine("== " + title + " ==");

    await using var command = connection.CreateCommand();
    command.CommandText = sql;
    command.CommandType = CommandType.Text;

    await using var reader = await command.ExecuteReaderAsync();
    PrintReader(reader);
}

static void PrintReader(IDataReader reader)
{
    for (var i = 0; i < reader.FieldCount; i++)
    {
        if (i > 0)
        {
            Console.Write("\t");
        }

        Console.Write(reader.GetName(i));
    }

    Console.WriteLine();

    var rowCount = 0;
    while (reader.Read())
    {
        rowCount++;
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (i > 0)
            {
                Console.Write("\t");
            }

            Console.Write(reader.IsDBNull(i) ? "" : Convert.ToString(reader.GetValue(i)));
        }

        Console.WriteLine();
    }

    Console.WriteLine($"-- rows: {rowCount}");
}

Imports System.Data
Imports System.Globalization
Imports System.Threading
Imports ConsultaPoliza.Api.Models
Imports Oracle.ManagedDataAccess.Client

Namespace ConsultaPoliza.Api.Services
    Public Interface IPolicyResponseBuilder
        Function Build(
            connection As OracleConnection,
            policy As PolicyBaseData,
            effectiveDate As DateTime?,
            cancellationToken As CancellationToken) As PolicyResponse
    End Interface

    Public Class PolicyResponseBuilder
        Implements IPolicyResponseBuilder

        Private Const RolesQuery As String =
            "SELECT R.NROLE, " &
            "       T.SDESCRIPT AS ROLE_DESCRIPTION, " &
            "       R.SCLIENT, " &
            "       C.SCLIENAME, " &
            "       R.DNULLDATE, " &
            "       R.DEFFECDATE " &
            "  FROM ROLES R " &
            "  LEFT JOIN TABLE12 T " &
            "         ON T.NROLE = R.NROLE " &
            "        AND T.SSTATREGT = '1' " &
            "  LEFT JOIN CLIENT C " &
            "         ON C.SCLIENT = R.SCLIENT " &
            " WHERE R.SCERTYPE = :certificateType " &
            "   AND R.NBRANCH = :branchCode " &
            "   AND R.NPRODUCT = :productCode " &
            "   AND R.NPOLICY = :policyNumber " &
            "   AND R.NCERTIF = :certificateNumber " &
            " ORDER BY R.NROLE, R.DEFFECDATE"

        Private Const RoleAddressesQuery As String =
            "SELECT A.SRECTYPE, " &
            "       CASE A.SRECTYPE " &
            "           WHEN '1' THEN 'Comercial' " &
            "           WHEN '2' THEN 'Particular' " &
            "           WHEN '3' THEN 'Casilla de correo' " &
            "           ELSE NULL " &
            "       END AS ADDRESS_TYPE_DESCRIPTION, " &
            "       A.SSTREET, " &
            "       COALESCE(A.SZIP_CODE, TO_CHAR(A.NZIP_CODE)) AS ZIP_CODE, " &
            "       A.NLOCAL, " &
            "       L.SDESCRIPT AS LOCAL_DESCRIPTION, " &
            "       A.NPROVINCE, " &
            "       P.SDESCRIPT AS PROVINCE_DESCRIPTION, " &
            "       A.NCOUNTRY, " &
            "       C.SDESCRIPT AS COUNTRY_DESCRIPTION, " &
            "       A.SE_MAIL " &
            "  FROM ADDRESS A " &
            "  LEFT JOIN TAB_LOCAT L " &
            "         ON L.NCOUNTRY = A.NCOUNTRY " &
            "        AND L.NPROVINCE = A.NPROVINCE " &
            "        AND L.NLOCAL = A.NLOCAL " &
            "  LEFT JOIN PROVINCE P " &
            "         ON P.NCOUNTRY = A.NCOUNTRY " &
            "        AND P.NPROVINCE = A.NPROVINCE " &
            "  LEFT JOIN TABLE66 C " &
            "         ON C.NCOUNTRY = A.NCOUNTRY " &
            "        AND C.SSTATREGT = '1' " &
            " WHERE A.SCLIENT = :clientCode " &
            "   AND A.DNULLDATE IS NULL " &
            " ORDER BY A.NRECOWNER, A.SRECTYPE, A.DEFFECDATE DESC"

        Private ReadOnly _reaGeneralPackage As IReaGeneralPackage

        Public Sub New(reaGeneralPackage As IReaGeneralPackage)
            _reaGeneralPackage = reaGeneralPackage
        End Sub

        Public Function Build(
            connection As OracleConnection,
            policy As PolicyBaseData,
            effectiveDate As DateTime?,
            cancellationToken As CancellationToken) As PolicyResponse Implements IPolicyResponseBuilder.Build

            Dim branch = _reaGeneralPackage.GetBranchDescription(connection, policy.RamoCodigo, cancellationToken)
            Dim product = _reaGeneralPackage.GetProductDescription(connection, policy.RamoCodigo, policy.ProductoCodigo, cancellationToken)
            Dim clientName = _reaGeneralPackage.GetClientName(connection, policy.ClienteCodigo, cancellationToken)
            Dim coverageEffectiveDate = If(effectiveDate, policy.FechaInicio)
            Dim primaryCoverage = _reaGeneralPackage.GetPrimaryCoverage(
                connection,
                policy.TipoCertificado,
                policy.RamoCodigo,
                policy.ProductoCodigo,
                policy.NumeroPoliza,
                coverageEffectiveDate,
                policy.TipoModulo,
                cancellationToken)
            Dim statusDetail As New PolicyStatusResponse(
                FormatCodeDescription(policy.EstadoDetalleCodigo, policy.EstadoDetalleDescripcion),
                FormatCodeDescription(policy.MotivoAnulacionCodigo, policy.MotivoAnulacionDescripcion),
                policy.FechaAnulacion,
                FormatCodeDescription(policy.MotivoSuspensionCodigo, policy.MotivoSuspensionDescripcion))
            Dim roles = GetRoles(connection, policy, cancellationToken)

            Return New PolicyResponse(
                policy.NumeroPoliza.ToString(),
                FormatCodeDescription(policy.EstadoCodigo, policy.EstadoDescripcion),
                clientName,
                product,
                policy.FechaInicio,
                policy.FechaFin,
                branch,
                policy.ClienteCodigo,
                primaryCoverage,
                policy.NumeroCertificado,
                policy.RamoCodigo,
                policy.ProductoCodigo,
                effectiveDate,
                FormatCodeDescription(policy.FrecuenciaPagoCodigo, policy.FrecuenciaPagoDescripcion),
                statusDetail,
                roles)
        End Function

        Private Shared Function GetRoles(
            connection As OracleConnection,
            policy As PolicyBaseData,
            cancellationToken As CancellationToken) As List(Of PolicyRoleResponse)

            Dim roles As New List(Of PolicyRoleResponse)()

            Using command = connection.CreateCommand()
                command.CommandText = RolesQuery
                command.CommandType = CommandType.Text
                command.BindByName = True
                command.Parameters.Add("certificateType", OracleDbType.Varchar2, policy.TipoCertificado, ParameterDirection.Input)
                command.Parameters.Add("branchCode", OracleDbType.Decimal, policy.RamoCodigo, ParameterDirection.Input)
                command.Parameters.Add("productCode", OracleDbType.Decimal, policy.ProductoCodigo, ParameterDirection.Input)
                command.Parameters.Add("policyNumber", OracleDbType.Decimal, policy.NumeroPoliza, ParameterDirection.Input)
                command.Parameters.Add("certificateNumber", OracleDbType.Decimal, policy.NumeroCertificado, ParameterDirection.Input)

                Using reader = command.ExecuteReader()
                    While reader.Read()
                        cancellationToken.ThrowIfCancellationRequested()
                        Dim clientCode = GetString(reader, "SCLIENT")

                        roles.Add(New PolicyRoleResponse(
                            FormatCodeDescription(GetIntegerOrNull(reader, "NROLE"), GetString(reader, "ROLE_DESCRIPTION")),
                            clientCode,
                            FormatCodeDescription(clientCode, GetString(reader, "SCLIENAME")),
                            GetDateTime(reader, "DNULLDATE"),
                            GetDateTime(reader, "DEFFECDATE"),
                            Nothing))
                    End While
                End Using
            End Using

            Dim addressCache As New Dictionary(Of String, List(Of PolicyRoleAddressResponse))(StringComparer.Ordinal)
            For Each role In roles
                If Not addressCache.ContainsKey(role.ClienteCodigo) Then
                    addressCache(role.ClienteCodigo) = GetRoleAddresses(connection, role.ClienteCodigo, cancellationToken)
                End If

                role.Direcciones = addressCache(role.ClienteCodigo)
            Next

            Return roles
        End Function

        Private Shared Function GetRoleAddresses(
            connection As OracleConnection,
            clientCode As String,
            cancellationToken As CancellationToken) As List(Of PolicyRoleAddressResponse)

            Dim addresses As New List(Of PolicyRoleAddressResponse)()

            If String.IsNullOrWhiteSpace(clientCode) Then
                Return addresses
            End If

            Using command = connection.CreateCommand()
                command.CommandText = RoleAddressesQuery
                command.CommandType = CommandType.Text
                command.BindByName = True
                command.Parameters.Add("clientCode", OracleDbType.Varchar2, clientCode, ParameterDirection.Input)

                Using reader = command.ExecuteReader()
                    While reader.Read()
                        cancellationToken.ThrowIfCancellationRequested()

                        addresses.Add(New PolicyRoleAddressResponse(
                            FormatCodeDescription(GetString(reader, "SRECTYPE"), GetString(reader, "ADDRESS_TYPE_DESCRIPTION")),
                            GetString(reader, "SSTREET"),
                            GetString(reader, "ZIP_CODE"),
                            FormatCodeDescription(GetIntegerOrNull(reader, "NLOCAL"), GetString(reader, "LOCAL_DESCRIPTION")),
                            FormatCodeDescription(GetIntegerOrNull(reader, "NPROVINCE"), GetString(reader, "PROVINCE_DESCRIPTION")),
                            GetString(reader, "COUNTRY_DESCRIPTION"),
                            GetString(reader, "SE_MAIL")))
                    End While
                End Using
            End Using

            Return addresses
        End Function

        Private Shared Function FormatCodeDescription(code As Integer?, description As String) As String
            If code.HasValue Then
                Return FormatCodeDescription(code.Value.ToString(), description)
            End If

            Return If(description, "").Trim()
        End Function

        Private Shared Function FormatCodeDescription(code As String, description As String) As String
            Dim cleanCode = If(code, "").Trim()
            Dim cleanDescription = If(description, "").Trim()

            If Not String.IsNullOrWhiteSpace(cleanCode) AndAlso Not String.IsNullOrWhiteSpace(cleanDescription) Then
                Return cleanCode & " - " & cleanDescription
            End If

            If Not String.IsNullOrWhiteSpace(cleanCode) Then
                Return cleanCode
            End If

            Return cleanDescription
        End Function

        Private Shared Function GetString(reader As OracleDataReader, columnName As String) As String
            Dim ordinal = reader.GetOrdinal(columnName)
            If reader.IsDBNull(ordinal) Then
                Return ""
            End If

            Return If(Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture), "").Trim()
        End Function

        Private Shared Function GetIntegerOrNull(reader As OracleDataReader, columnName As String) As Integer?
            Dim ordinal = reader.GetOrdinal(columnName)
            If reader.IsDBNull(ordinal) Then
                Return Nothing
            End If

            Return Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture)
        End Function

        Private Shared Function GetDateTime(reader As OracleDataReader, columnName As String) As DateTime?
            Dim ordinal = reader.GetOrdinal(columnName)
            If reader.IsDBNull(ordinal) Then
                Return Nothing
            End If

            Return Convert.ToDateTime(reader.GetValue(ordinal), CultureInfo.InvariantCulture)
        End Function
    End Class
End Namespace

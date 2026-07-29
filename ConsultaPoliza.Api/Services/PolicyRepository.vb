Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.Threading
Imports System.Threading.Tasks
Imports ConsultaPoliza.Api.Models
Imports ConsultaPoliza.Api.Options
Imports Oracle.ManagedDataAccess.Client

Namespace ConsultaPoliza.Api.Services
    Public Interface IPolicyRepository
        Function GetByNumberAsync(policyNumber As String, cancellationToken As CancellationToken) As Task(Of PolicyResponse)

        Function SearchAsync(criteria As PolicySearchCriteria, cancellationToken As CancellationToken) As Task(Of PolicyResponse)
    End Interface

    Public Class OraclePolicyRepository
        Implements IPolicyRepository

        Private Const PolicyByNumberQuery As String =
            "SELECT PO.NPOLICY, CE.NCERTIF, PO.SCERTYPE, PO.NBRANCH, " &
            "       CE.NPRODUCT, CE.SCLIENT, CE.DSTARTDATE, CE.DEXPIRDAT, " &
            "       PO.STYP_MODULE, " &
            "       COALESCE(CE.SSTATUSVA, PO.SSTATUS_POL) AS STATUS_CODE, " &
            "       ST.SDESCRIPT AS STATUS_DESCRIPTION, " &
            "       CS.NFACESTATUS AS FACE_STATUS_CODE, " &
            "       FS.SDESCRIPT AS FACE_STATUS_DESCRIPTION, " &
            "       COALESCE(CE.NNULLCODE, PO.NNULLCODE) AS NULL_CODE, " &
            "       NC.SDESCRIPT AS NULL_DESCRIPTION, " &
            "       COALESCE(CE.DNULLDATE, PO.DNULLDATE) AS NULL_DATE, " &
            "       CE.NSUS_REASON AS SUSPENSION_REASON_CODE, " &
            "       SR.SDESCRIPT AS SUSPENSION_REASON_DESCRIPTION, " &
            "       COALESCE(CE.NPAYFREQ, PO.NPAYFREQ) AS PAY_FREQUENCY_CODE, " &
            "       PF.SDESCRIPT AS PAY_FREQUENCY_DESCRIPTION " &
            "  FROM POLICY PO " &
            "  INNER JOIN CERTIFICAT CE " &
            "          ON CE.NBRANCH = PO.NBRANCH " &
            "         AND CE.NPOLICY = PO.NPOLICY " &
            "         AND CE.SCERTYPE = PO.SCERTYPE " &
            "         AND CE.SCLIENT = PO.SCLIENT " &
            "  LEFT JOIN TABLE181 ST " &
            "         ON ST.SSTATUSVA = COALESCE(CE.SSTATUSVA, PO.SSTATUS_POL) " &
            "        AND ST.SSTATREGT = '1' " &
            "  LEFT JOIN CERT_STATUS CS " &
            "         ON CS.SCERTYPE = CE.SCERTYPE " &
            "        AND CS.NBRANCH = CE.NBRANCH " &
            "        AND CS.NPRODUCT = CE.NPRODUCT " &
            "        AND CS.NPOLICY = CE.NPOLICY " &
            "        AND CS.NCERTIF = CE.NCERTIF " &
            "  LEFT JOIN TABLE6765 FS " &
            "         ON FS.NFACESTATUS = CS.NFACESTATUS " &
            "        AND FS.SSTATREGT = '1' " &
            "  LEFT JOIN TABLE13 NC " &
            "         ON NC.NNULLCODE = COALESCE(CE.NNULLCODE, PO.NNULLCODE) " &
            "        AND NC.SSTATREGT = '1' " &
            "  LEFT JOIN TABLE5566 SR " &
            "         ON SR.NSUS_REASON = CE.NSUS_REASON " &
            "        AND SR.SSTATREGT = '1' " &
            "  LEFT JOIN TABLE36 PF " &
            "         ON PF.NPAYFREQ = COALESCE(CE.NPAYFREQ, PO.NPAYFREQ) " &
            "        AND PF.SSTATREGT = '1' " &
            " WHERE PO.NPOLICY = :policyNumber " &
            " ORDER BY CASE WHEN CE.NCERTIF = 0 THEN 0 ELSE 1 END, CE.NCERTIF " &
            " FETCH FIRST 1 ROWS ONLY"

        Private Const PolicySearchQuery As String =
            "SELECT PO.NPOLICY, CE.NCERTIF, PO.SCERTYPE, PO.NBRANCH, " &
            "       CE.NPRODUCT, CE.SCLIENT, CE.DSTARTDATE, CE.DEXPIRDAT, " &
            "       PO.STYP_MODULE, " &
            "       COALESCE(CE.SSTATUSVA, PO.SSTATUS_POL) AS STATUS_CODE, " &
            "       ST.SDESCRIPT AS STATUS_DESCRIPTION, " &
            "       CS.NFACESTATUS AS FACE_STATUS_CODE, " &
            "       FS.SDESCRIPT AS FACE_STATUS_DESCRIPTION, " &
            "       COALESCE(CE.NNULLCODE, PO.NNULLCODE) AS NULL_CODE, " &
            "       NC.SDESCRIPT AS NULL_DESCRIPTION, " &
            "       COALESCE(CE.DNULLDATE, PO.DNULLDATE) AS NULL_DATE, " &
            "       CE.NSUS_REASON AS SUSPENSION_REASON_CODE, " &
            "       SR.SDESCRIPT AS SUSPENSION_REASON_DESCRIPTION, " &
            "       COALESCE(CE.NPAYFREQ, PO.NPAYFREQ) AS PAY_FREQUENCY_CODE, " &
            "       PF.SDESCRIPT AS PAY_FREQUENCY_DESCRIPTION " &
            "  FROM POLICY PO " &
            "  INNER JOIN CERTIFICAT CE " &
            "          ON CE.NBRANCH = PO.NBRANCH " &
            "         AND CE.NPOLICY = PO.NPOLICY " &
            "         AND CE.SCERTYPE = PO.SCERTYPE " &
            "         AND CE.SCLIENT = PO.SCLIENT " &
            "  LEFT JOIN TABLE181 ST " &
            "         ON ST.SSTATUSVA = COALESCE(CE.SSTATUSVA, PO.SSTATUS_POL) " &
            "        AND ST.SSTATREGT = '1' " &
            "  LEFT JOIN CERT_STATUS CS " &
            "         ON CS.SCERTYPE = CE.SCERTYPE " &
            "        AND CS.NBRANCH = CE.NBRANCH " &
            "        AND CS.NPRODUCT = CE.NPRODUCT " &
            "        AND CS.NPOLICY = CE.NPOLICY " &
            "        AND CS.NCERTIF = CE.NCERTIF " &
            "  LEFT JOIN TABLE6765 FS " &
            "         ON FS.NFACESTATUS = CS.NFACESTATUS " &
            "        AND FS.SSTATREGT = '1' " &
            "  LEFT JOIN TABLE13 NC " &
            "         ON NC.NNULLCODE = COALESCE(CE.NNULLCODE, PO.NNULLCODE) " &
            "        AND NC.SSTATREGT = '1' " &
            "  LEFT JOIN TABLE5566 SR " &
            "         ON SR.NSUS_REASON = CE.NSUS_REASON " &
            "        AND SR.SSTATREGT = '1' " &
            "  LEFT JOIN TABLE36 PF " &
            "         ON PF.NPAYFREQ = COALESCE(CE.NPAYFREQ, PO.NPAYFREQ) " &
            "        AND PF.SSTATREGT = '1' " &
            " WHERE PO.NBRANCH = :branchCode " &
            "   AND PO.NPOLICY = :policyNumber " &
            "   AND CE.NCERTIF = :certificateNumber " &
            " ORDER BY CE.DSTARTDATE DESC " &
            " FETCH FIRST 1 ROWS ONLY"

        Private ReadOnly _options As OraclePolicyOptions
        Private ReadOnly _policyResponseBuilder As IPolicyResponseBuilder

        Public Sub New(options As OraclePolicyOptions, policyResponseBuilder As IPolicyResponseBuilder)
            _options = options
            _policyResponseBuilder = policyResponseBuilder
        End Sub

        Public Function GetByNumberAsync(policyNumber As String, cancellationToken As CancellationToken) As Task(Of PolicyResponse) Implements IPolicyRepository.GetByNumberAsync
            Dim parsedPolicyNumber = Long.Parse(policyNumber, CultureInfo.InvariantCulture)

            Return Task.Run(
                Function()
                    Return Query(
                        PolicyByNumberQuery,
                        Sub(command)
                            command.Parameters.Add("policyNumber", OracleDbType.Decimal, parsedPolicyNumber, ParameterDirection.Input)
                        End Sub,
                        Nothing,
                        cancellationToken)
                End Function,
                cancellationToken)
        End Function

        Public Function SearchAsync(criteria As PolicySearchCriteria, cancellationToken As CancellationToken) As Task(Of PolicyResponse) Implements IPolicyRepository.SearchAsync
            Return Task.Run(
                Function()
                    Return Query(
                        PolicySearchQuery,
                        Sub(command)
                            command.Parameters.Add("branchCode", OracleDbType.Decimal, criteria.RamoCodigo, ParameterDirection.Input)
                            command.Parameters.Add("policyNumber", OracleDbType.Decimal, criteria.NumeroPoliza, ParameterDirection.Input)
                            command.Parameters.Add("certificateNumber", OracleDbType.Decimal, criteria.NumeroCertificado, ParameterDirection.Input)
                        End Sub,
                        criteria.FechaEfecto,
                        cancellationToken)
                End Function,
                cancellationToken)
        End Function

        Private Function Query(
            commandText As String,
            addParameters As Action(Of OracleCommand),
            effectiveDate As DateTime?,
            cancellationToken As CancellationToken) As PolicyResponse

            If String.IsNullOrWhiteSpace(_options.ConnectionString) Then
                Throw New InvalidOperationException("OraclePolicy:ConnectionString is not configured.")
            End If

            cancellationToken.ThrowIfCancellationRequested()

            Using connection As New OracleConnection(_options.ConnectionString)
                connection.Open()
                OracleReadOnlySession.Begin(connection, cancellationToken)

                Try
                    Dim baseData As PolicyBaseData

                    Using command = connection.CreateCommand()
                        command.CommandText = commandText
                        command.CommandType = CommandType.Text
                        command.BindByName = True
                        addParameters(command)

                        Using reader = command.ExecuteReader(CommandBehavior.SingleRow)
                            cancellationToken.ThrowIfCancellationRequested()
                            If Not reader.Read() Then
                                Return Nothing
                            End If

                            baseData = New PolicyBaseData(
                                GetLong(reader, "NPOLICY"),
                                GetInt(reader, "NCERTIF"),
                                GetString(reader, "SCERTYPE"),
                                GetInt(reader, "NBRANCH"),
                                GetInt(reader, "NPRODUCT"),
                                GetString(reader, "SCLIENT"),
                                GetDateTime(reader, "DSTARTDATE"),
                                GetDateTime(reader, "DEXPIRDAT"),
                                GetStringOrNull(reader, "STYP_MODULE"),
                                GetString(reader, "STATUS_CODE"),
                                GetString(reader, "STATUS_DESCRIPTION"),
                                GetIntOrNull(reader, "FACE_STATUS_CODE"),
                                GetString(reader, "FACE_STATUS_DESCRIPTION"),
                                GetIntOrNull(reader, "NULL_CODE"),
                                GetString(reader, "NULL_DESCRIPTION"),
                                GetDateTime(reader, "NULL_DATE"),
                                GetIntOrNull(reader, "SUSPENSION_REASON_CODE"),
                                GetString(reader, "SUSPENSION_REASON_DESCRIPTION"),
                                GetIntOrNull(reader, "PAY_FREQUENCY_CODE"),
                                GetString(reader, "PAY_FREQUENCY_DESCRIPTION"))
                        End Using
                    End Using

                    Return _policyResponseBuilder.Build(connection, baseData, effectiveDate, cancellationToken)
                Finally
                    OracleReadOnlySession.Rollback(connection)
                End Try
            End Using
        End Function

        Private Shared Function GetString(reader As DbDataReader, ParamArray names As String()) As String
            Dim ordinal As Integer
            If Not TryGetOrdinal(reader, names, ordinal) OrElse reader.IsDBNull(ordinal) Then
                Return ""
            End If

            Return If(Convert.ToString(reader.GetValue(ordinal)), "")
        End Function

        Private Shared Function GetDateTime(reader As DbDataReader, ParamArray names As String()) As DateTime?
            Dim ordinal As Integer
            If Not TryGetOrdinal(reader, names, ordinal) OrElse reader.IsDBNull(ordinal) Then
                Return Nothing
            End If

            Return Convert.ToDateTime(reader.GetValue(ordinal), CultureInfo.InvariantCulture)
        End Function

        Private Shared Function GetStringOrNull(reader As DbDataReader, ParamArray names As String()) As String
            Dim ordinal As Integer
            If Not TryGetOrdinal(reader, names, ordinal) OrElse reader.IsDBNull(ordinal) Then
                Return Nothing
            End If

            Return Convert.ToString(reader.GetValue(ordinal))
        End Function

        Private Shared Function GetInt(reader As DbDataReader, ParamArray names As String()) As Integer
            Dim ordinal As Integer
            If Not TryGetOrdinal(reader, names, ordinal) OrElse reader.IsDBNull(ordinal) Then
                Throw New InvalidOperationException($"Oracle result cursor did not return required number field: {String.Join(", ", names)}.")
            End If

            Return Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture)
        End Function

        Private Shared Function GetIntOrNull(reader As DbDataReader, ParamArray names As String()) As Integer?
            Dim ordinal As Integer
            If Not TryGetOrdinal(reader, names, ordinal) OrElse reader.IsDBNull(ordinal) Then
                Return Nothing
            End If

            Return Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture)
        End Function

        Private Shared Function GetLong(reader As DbDataReader, ParamArray names As String()) As Long
            Dim ordinal As Integer
            If Not TryGetOrdinal(reader, names, ordinal) OrElse reader.IsDBNull(ordinal) Then
                Throw New InvalidOperationException($"Oracle result cursor did not return required number field: {String.Join(", ", names)}.")
            End If

            Return Convert.ToInt64(reader.GetValue(ordinal), CultureInfo.InvariantCulture)
        End Function

        Private Shared Function TryGetOrdinal(reader As DbDataReader, names As String(), ByRef ordinal As Integer) As Boolean
            For index = 0 To reader.FieldCount - 1
                Dim columnName = reader.GetName(index)
                If names.Any(Function(name) String.Equals(name, columnName, StringComparison.OrdinalIgnoreCase)) Then
                    ordinal = index
                    Return True
                End If
            Next

            ordinal = -1
            Return False
        End Function
    End Class
End Namespace

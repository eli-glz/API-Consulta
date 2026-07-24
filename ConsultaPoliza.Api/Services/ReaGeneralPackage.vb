Imports System.Data
Imports System.Globalization
Imports System.Threading
Imports Oracle.ManagedDataAccess.Client

Namespace ConsultaPoliza.Api.Services
    Public Interface IReaGeneralPackage
        Function GetBranchDescription(connection As OracleConnection, branchCode As Integer, cancellationToken As CancellationToken) As String

        Function GetProductDescription(connection As OracleConnection, branchCode As Integer, productCode As Integer, cancellationToken As CancellationToken) As String

        Function GetClientName(connection As OracleConnection, clientCode As String, cancellationToken As CancellationToken) As String

        Function GetPrimaryCoverage(
            connection As OracleConnection,
            certificateType As String,
            branchCode As Integer,
            productCode As Integer,
            policyNumber As Long,
            effectiveDate As DateTime?,
            moduleType As String,
            cancellationToken As CancellationToken) As Integer?
    End Interface

    Public Class ReaGeneralPackage
        Implements IReaGeneralPackage

        Public Function GetBranchDescription(connection As OracleConnection, branchCode As Integer, cancellationToken As CancellationToken) As String Implements IReaGeneralPackage.GetBranchDescription
            Return ExecuteStringFunction(
                connection,
                "REAGENERALPKG.REASBRANCH",
                200,
                cancellationToken,
                Sub(command)
                    command.Parameters.Add("NBRANCH", OracleDbType.Decimal, branchCode, ParameterDirection.Input)
                End Sub)
        End Function

        Public Function GetProductDescription(connection As OracleConnection, branchCode As Integer, productCode As Integer, cancellationToken As CancellationToken) As String Implements IReaGeneralPackage.GetProductDescription
            Return ExecuteStringFunction(
                connection,
                "REAGENERALPKG.REASPRODUCT",
                200,
                cancellationToken,
                Sub(command)
                    command.Parameters.Add("NBRANCH", OracleDbType.Decimal, branchCode, ParameterDirection.Input)
                    command.Parameters.Add("NPRODUCT", OracleDbType.Decimal, productCode, ParameterDirection.Input)
                End Sub)
        End Function

        Public Function GetClientName(connection As OracleConnection, clientCode As String, cancellationToken As CancellationToken) As String Implements IReaGeneralPackage.GetClientName
            Return ExecuteStringFunction(
                connection,
                "REAGENERALPKG.REANAMECLI",
                200,
                cancellationToken,
                Sub(command)
                    command.Parameters.Add("SCLIENT", OracleDbType.Varchar2, clientCode, ParameterDirection.Input)
                End Sub)
        End Function

        Public Function GetPrimaryCoverage(
            connection As OracleConnection,
            certificateType As String,
            branchCode As Integer,
            productCode As Integer,
            policyNumber As Long,
            effectiveDate As DateTime?,
            moduleType As String,
            cancellationToken As CancellationToken) As Integer? Implements IReaGeneralPackage.GetPrimaryCoverage

            If Not effectiveDate.HasValue Then
                Return Nothing
            End If

            Using command = connection.CreateCommand()
                command.CommandText = "REAGENERALPKG.REACOVER_PPAL"
                command.CommandType = CommandType.StoredProcedure

                Dim returnValue = command.Parameters.Add("return_value", OracleDbType.Decimal)
                returnValue.Direction = ParameterDirection.ReturnValue

                command.Parameters.Add("SCERTYPE", OracleDbType.Varchar2, certificateType, ParameterDirection.Input)
                command.Parameters.Add("NBRANCH", OracleDbType.Decimal, branchCode, ParameterDirection.Input)
                command.Parameters.Add("NPRODUCT", OracleDbType.Decimal, productCode, ParameterDirection.Input)
                command.Parameters.Add("NPOLICY", OracleDbType.Decimal, policyNumber, ParameterDirection.Input)
                command.Parameters.Add("DEFFECDATE", OracleDbType.Date, effectiveDate.Value, ParameterDirection.Input)

                Dim moduleValue As Object = If(String.IsNullOrWhiteSpace(moduleType), CType(DBNull.Value, Object), moduleType)
                command.Parameters.Add("STYP_MODULE", OracleDbType.Varchar2, moduleValue, ParameterDirection.Input)

                cancellationToken.ThrowIfCancellationRequested()
                command.ExecuteNonQuery()

                If returnValue.Value Is Nothing OrElse returnValue.Value Is DBNull.Value Then
                    Return Nothing
                End If

                Return Convert.ToInt32(returnValue.Value.ToString(), CultureInfo.InvariantCulture)
            End Using
        End Function

        Private Shared Function ExecuteStringFunction(
            connection As OracleConnection,
            functionName As String,
            returnSize As Integer,
            cancellationToken As CancellationToken,
            addInputParameters As Action(Of OracleCommand)) As String

            Using command = connection.CreateCommand()
                command.CommandText = functionName
                command.CommandType = CommandType.StoredProcedure

                Dim returnValue = command.Parameters.Add("return_value", OracleDbType.Varchar2, returnSize)
                returnValue.Direction = ParameterDirection.ReturnValue

                addInputParameters(command)

                cancellationToken.ThrowIfCancellationRequested()
                command.ExecuteNonQuery()
                Return If(Convert.ToString(returnValue.Value), "").Trim()
            End Using
        End Function
    End Class
End Namespace

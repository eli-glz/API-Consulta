Imports System.Configuration
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace ConsultaPoliza.Api.Options
    Public Class OraclePolicyOptions
        Public Const SectionName As String = "OraclePolicy"
        Private Const DevelopmentUserSecretsId As String = "consulta-poliza-api-dev"
        Private Const ConnectionStringKey As String = "OraclePolicy:ConnectionString"

        Public Property ConnectionString As String = ""

        Public Shared Function FromConfiguration() As OraclePolicyOptions
            Dim value = Environment.GetEnvironmentVariable("ORACLE_POLICY_CONNECTION")

            If String.IsNullOrWhiteSpace(value) Then
                value = ConfigurationManager.AppSettings(ConnectionStringKey)
            End If

            If String.IsNullOrWhiteSpace(value) Then
                value = ConfigurationManager.AppSettings("OraclePolicy.ConnectionString")
            End If

            If String.IsNullOrWhiteSpace(value) AndAlso ConfigurationManager.ConnectionStrings("OraclePolicy") IsNot Nothing Then
                value = ConfigurationManager.ConnectionStrings("OraclePolicy").ConnectionString
            End If

            If String.IsNullOrWhiteSpace(value) Then
                value = ReadDevelopmentUserSecret()
            End If

            Return New OraclePolicyOptions With {
                .ConnectionString = If(value, "")
            }
        End Function

        Private Shared Function ReadDevelopmentUserSecret() As String
            Dim appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            If String.IsNullOrWhiteSpace(appDataPath) Then
                Return ""
            End If

            Dim secretsPath = Path.Combine(
                appDataPath,
                "Microsoft",
                "UserSecrets",
                DevelopmentUserSecretsId,
                "secrets.json")

            If Not File.Exists(secretsPath) Then
                Return ""
            End If

            Try
                Dim secrets = JObject.Parse(File.ReadAllText(secretsPath))
                Return If(secrets.Value(Of String)(ConnectionStringKey), "")
            Catch ex As IOException
                Return ""
            Catch ex As UnauthorizedAccessException
                Return ""
            Catch ex As JsonException
                Return ""
            End Try
        End Function
    End Class
End Namespace

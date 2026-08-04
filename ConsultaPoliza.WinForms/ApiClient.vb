Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Json
Imports System.Globalization
Imports System.Text.Json

Public Class ApiClient
    Implements IDisposable
    Private ReadOnly _httpClient As HttpClient

    Public Sub New(baseUrl As String)
        _httpClient = New HttpClient() With {
            .BaseAddress = New Uri(baseUrl.TrimEnd("/"c) & "/")
        }
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        _httpClient.Dispose()
    End Sub

    Public Async Function GetBranchesAsync() As Task(Of List(Of BranchResponse))
        Dim response = Await _httpClient.GetAsync("api/ramos")
        Await EnsureSuccessAsync(response)

        Dim branches = Await response.Content.ReadFromJsonAsync(Of List(Of BranchResponse))()
        Return If(branches, New List(Of BranchResponse)())
    End Function

    Public Async Function GetPolicyAsync(
        branchCode As Integer,
        policyNumber As String,
        certificateNumber As Integer,
        effectiveDate As DateTime
    ) As Task(Of PolicyResponse)
        Dim query = String.Format(
            CultureInfo.InvariantCulture,
            "api/polizas?ramo={0}&numeroPoliza={1}&certificado={2}&fechaEfecto={3}",
            branchCode,
            Uri.EscapeDataString(policyNumber),
            certificateNumber,
            effectiveDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))

        Dim response = Await _httpClient.GetAsync(query)

        If response.StatusCode = HttpStatusCode.NotFound Then
            Return Nothing
        End If

        Await EnsureSuccessAsync(response)
        Return Await response.Content.ReadFromJsonAsync(Of PolicyResponse)()
    End Function

    Private Shared Async Function EnsureSuccessAsync(response As HttpResponseMessage) As Task
        If response.IsSuccessStatusCode Then
            Return
        End If

        Dim message = Await ReadErrorMessageAsync(response)

        Throw New InvalidOperationException(message)
    End Function

    Private Shared Async Function ReadErrorMessageAsync(response As HttpResponseMessage) As Task(Of String)
        Dim content = Await response.Content.ReadAsStringAsync()

        If Not String.IsNullOrWhiteSpace(content) Then
            Try
                Using document = JsonDocument.Parse(content)
                    Dim detail As JsonElement
                    If document.RootElement.TryGetProperty("detail", detail) Then
                        Dim detailMessage = detail.GetString()
                        If Not String.IsNullOrWhiteSpace(detailMessage) Then
                            Return detailMessage
                        End If
                    End If

                    Dim message As JsonElement
                    If document.RootElement.TryGetProperty("message", message) Then
                        Dim errorMessage = message.GetString()
                        If Not String.IsNullOrWhiteSpace(errorMessage) Then
                            Return errorMessage
                        End If
                    End If
                End Using
            Catch ex As JsonException
                Return content
            End Try
        End If

        Return "La API devolvio el estado " & CInt(response.StatusCode).ToString() & "."
    End Function
End Class

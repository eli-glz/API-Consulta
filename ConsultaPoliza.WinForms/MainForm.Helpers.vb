Imports System.Globalization

Partial Public Class MainForm
    Private Sub ClearResult()
        _currentPolicy = Nothing
        InitializeNavigationTree(Nothing)
        ShowNoPolicySelected()
    End Sub

    Private Shared Function FormatCodeDescription(code As Integer?, description As String) As String
        If code.HasValue AndAlso Not String.IsNullOrWhiteSpace(description) Then
            Return code.Value.ToString() & " - " & description.Trim()
        End If

        If code.HasValue Then
            Return code.Value.ToString()
        End If

        Return If(description, "").Trim()
    End Function

    Private Shared Function FormatAmount(value As Decimal?) As String
        If Not value.HasValue Then
            Return ""
        End If

        Return value.Value.ToString(
            "N2",
            CultureInfo.GetCultureInfo("es-AR"))
    End Function

    Private Shared Function FormatDate(value As DateTime?) As String
        If Not value.HasValue Then
            Return ""
        End If

        Return value.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("es-AR"))
    End Function

    Private Shared Function FormatNullableInteger(value As Integer?) As String
        If Not value.HasValue Then
            Return ""
        End If

        Return value.Value.ToString(CultureInfo.InvariantCulture)
    End Function

    Private Function FindRoleForNode(node As TreeNode) As PolicyRoleResponse
        If node Is Nothing OrElse _currentPolicy Is Nothing OrElse _currentPolicy.Roles Is Nothing Then
            Return Nothing
        End If

        Dim roleNode = If(String.Equals(TryCast(node.Tag, String), NodeRoleDirections, StringComparison.Ordinal), node.Parent, node)
        If roleNode Is Nothing OrElse roleNode.Parent Is Nothing Then
            Return Nothing
        End If

        Dim index = roleNode.Index
        If index < 0 OrElse index >= _currentPolicy.Roles.Count Then
            Return Nothing
        End If

        Return _currentPolicy.Roles(index)
    End Function

    Private Shared Function FormatClient(policy As PolicyResponse) As String
        If policy Is Nothing Then
            Return ""
        End If

        If HasText(policy.NumeroCliente) AndAlso HasText(policy.Asegurado) Then
            Return policy.NumeroCliente.Trim() & " - " & policy.Asegurado.Trim()
        End If

        If HasText(policy.NumeroCliente) Then
            Return policy.NumeroCliente.Trim()
        End If

        Return If(policy.Asegurado, "").Trim()
    End Function

    Private Shared Function HasText(value As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(value)
    End Function
End Class
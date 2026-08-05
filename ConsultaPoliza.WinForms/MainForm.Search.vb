Imports System.Globalization

Partial Public Class MainForm
    Private Async Sub MainForm_Shown(sender As Object, e As EventArgs)
        Await LoadBranchesAsync()
    End Sub

    Private Async Function LoadBranchesAsync() As Task
        SetBusy(True, "Cargando ramos...")

        Try
            Dim branches = Await _apiClient.GetBranchesAsync()
            _branchComboBox.DataSource = branches

            If branches.Count = 0 Then
                SetStatus("No se encontraron ramos disponibles.", True)
                Return
            End If

            _branchComboBox.SelectedIndex = 0
            SetStatus("")
        Catch ex As Exception
            SetStatus(ex.Message, True)
        Finally
            SetBusy(False)
        End Try
    End Function

    Private Async Sub SearchButton_Click(sender As Object, e As EventArgs)
        Dim selectedBranch = TryCast(_branchComboBox.SelectedItem, BranchResponse)
        If selectedBranch Is Nothing Then
            SetStatus("Seleccione un ramo.", True)
            Return
        End If

        Dim policyNumber = _policyNumberTextBox.Text.Trim()
        Dim parsedPolicyNumber As Long
        If Not Long.TryParse(policyNumber, NumberStyles.None, CultureInfo.InvariantCulture, parsedPolicyNumber) OrElse parsedPolicyNumber <= 0 Then
            SetStatus("Ingrese un numero de poliza valido.", True)
            _policyNumberTextBox.Focus()
            Return
        End If

        Dim certificateNumber = Decimal.ToInt32(_certificateNumeric.Value)
        Dim effectiveDate = _effectiveDatePicker.Value.Date

        ClearResult()
        SetBusy(True, "Consultando poliza...")

        Try
            Dim policy = Await _apiClient.GetPolicyAsync(
                selectedBranch.Codigo,
                policyNumber,
                certificateNumber,
                effectiveDate)

            If policy Is Nothing Then
                SetStatus("No se encontro una poliza con los criterios indicados.", True)
                Return
            End If

            _currentPolicy = policy
            _receiptsPageIndex = 0
            InitializeNavigationTree(policy)
            ShowPolicyOverview()
            SetStatus("Consulta realizada.")
        Catch ex As Exception
            SetStatus(ex.Message, True)
        Finally
            SetBusy(False)
        End Try
    End Sub
End Class
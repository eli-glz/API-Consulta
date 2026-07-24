Imports System.Globalization

Public Class MainForm
    Inherits Form

    Private ReadOnly _apiClient As ApiClient
    Private ReadOnly _branchComboBox As New ComboBox()
    Private ReadOnly _policyNumberTextBox As New TextBox()
    Private ReadOnly _certificateNumeric As New NumericUpDown()
    Private ReadOnly _effectiveDatePicker As New DateTimePicker()
    Private ReadOnly _searchButton As New Button()
    Private ReadOnly _statusLabel As New Label()

    Private ReadOnly _ramoValue As New Label()
    Private ReadOnly _numeroValue As New Label()
    Private ReadOnly _certificadoValue As New Label()
    Private ReadOnly _productoValue As New Label()
    Private ReadOnly _estadoValue As New Label()
    Private ReadOnly _aseguradoValue As New Label()
    Private ReadOnly _desdeValue As New Label()
    Private ReadOnly _hastaValue As New Label()
    Private ReadOnly _clienteValue As New Label()
    Private ReadOnly _coberturaValue As New Label()

    Public Sub New(apiClient As ApiClient)
        _apiClient = apiClient
        Text = "Consulta de Poliza"
        StartPosition = FormStartPosition.CenterScreen
        BackColor = Color.White
        Font = New Font("Segoe UI", 10.0F)
        ClientSize = New Size(1080, 1)

        BuildLayout()
        AddHandler Shown, AddressOf MainForm_Shown
    End Sub

    Private Sub BuildLayout()
        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .Padding = New Padding(20),
            .ColumnCount = 1,
            .RowCount = 4,
            .BackColor = Color.White
        }

        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 36))
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))

        Dim searchLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .ColumnCount = 4,
            .RowCount = 3,
            .Margin = New Padding(0)
        }

        searchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120))
        searchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 55))
        searchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150))
        searchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 45))
        searchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
        searchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
        searchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 54))

        ConfigureBranchComboBox()
        ConfigurePolicyNumberTextBox()
        ConfigureCertificateNumeric()
        ConfigureEffectiveDatePicker()
        ConfigureSearchButton()

        searchLayout.Controls.Add(CreateInputLabel("Ramo"), 0, 0)
        searchLayout.Controls.Add(_branchComboBox, 1, 0)
        searchLayout.Controls.Add(CreateInputLabel("Poliza"), 2, 0)
        searchLayout.Controls.Add(_policyNumberTextBox, 3, 0)
        searchLayout.Controls.Add(CreateInputLabel("Certificado"), 0, 1)
        searchLayout.Controls.Add(_certificateNumeric, 1, 1)
        searchLayout.Controls.Add(CreateInputLabel("Fecha de efecto"), 2, 1)
        searchLayout.Controls.Add(_effectiveDatePicker, 3, 1)

        Dim actions As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Padding = New Padding(0, 8, 0, 0),
            .Margin = New Padding(0)
        }
        actions.Controls.Add(_searchButton)
        searchLayout.Controls.Add(actions, 0, 2)
        searchLayout.SetColumnSpan(actions, 4)

        _statusLabel.Dock = DockStyle.Fill
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft
        _statusLabel.ForeColor = Color.DimGray
        _statusLabel.AutoEllipsis = True

        Dim resultHeader As New Label With {
            .Dock = DockStyle.Fill,
            .Text = "Poliza",
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.FromArgb(232, 236, 242),
            .ForeColor = Color.FromArgb(35, 42, 52),
            .Font = New Font(Font, FontStyle.Bold),
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim resultLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .ColumnCount = 2,
            .RowCount = 10,
            .Margin = New Padding(0),
            .Padding = New Padding(0, 6, 0, 0)
        }
        resultLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260))
        resultLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))

        AddResultRow(resultLayout, 0, "Ramo", _ramoValue)
        AddResultRow(resultLayout, 1, "Poliza", _numeroValue)
        AddResultRow(resultLayout, 2, "Certificado", _certificadoValue)
        AddResultRow(resultLayout, 3, "Producto", _productoValue)
        AddResultRow(resultLayout, 4, "Estado", _estadoValue)
        AddResultRow(resultLayout, 5, "Asegurado", _aseguradoValue)
        AddResultRow(resultLayout, 6, "Inicio de vigencia", _desdeValue)
        AddResultRow(resultLayout, 7, "Fin de vigencia", _hastaValue)
        AddResultRow(resultLayout, 8, "Numero cliente", _clienteValue)
        AddResultRow(resultLayout, 9, "Cobertura principal", _coberturaValue)

        root.Controls.Add(searchLayout, 0, 0)
        root.Controls.Add(_statusLabel, 0, 1)
        root.Controls.Add(resultHeader, 0, 2)
        root.Controls.Add(resultLayout, 0, 3)

        Controls.Add(root)
        AcceptButton = _searchButton

        PerformLayout()
        ClientSize = New Size(ClientSize.Width, root.PreferredSize.Height)
    End Sub

    Private Sub ConfigureBranchComboBox()
        _branchComboBox.Dock = DockStyle.Fill
        _branchComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        _branchComboBox.DisplayMember = NameOf(BranchResponse.DisplayText)
        _branchComboBox.ValueMember = NameOf(BranchResponse.Codigo)
        _branchComboBox.IntegralHeight = False
        _branchComboBox.DropDownHeight = 260
        _branchComboBox.Enabled = False
    End Sub

    Private Sub ConfigurePolicyNumberTextBox()
        _policyNumberTextBox.Dock = DockStyle.Fill
        _policyNumberTextBox.MaxLength = 18
        _policyNumberTextBox.TextAlign = HorizontalAlignment.Right
    End Sub

    Private Sub ConfigureCertificateNumeric()
        _certificateNumeric.Dock = DockStyle.Fill
        _certificateNumeric.Minimum = 0D
        _certificateNumeric.Maximum = 999999999D
        _certificateNumeric.DecimalPlaces = 0
        _certificateNumeric.TextAlign = HorizontalAlignment.Right
        _certificateNumeric.Value = 0D
    End Sub

    Private Sub ConfigureEffectiveDatePicker()
        _effectiveDatePicker.Dock = DockStyle.Fill
        _effectiveDatePicker.Format = DateTimePickerFormat.Custom
        _effectiveDatePicker.CustomFormat = "dd/MM/yyyy"
        _effectiveDatePicker.Value = Date.Today
    End Sub

    Private Sub ConfigureSearchButton()
        _searchButton.Text = "Buscar"
        _searchButton.Size = New Size(100, 34)
        _searchButton.Enabled = False
        AddHandler _searchButton.Click, AddressOf SearchButton_Click
    End Sub

    Private Shared Function CreateInputLabel(text As String) As Label
        Return New Label With {
            .Dock = DockStyle.Fill,
            .Text = text,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Margin = New Padding(0)
        }
    End Function

    Private Sub AddResultRow(layout As TableLayoutPanel, row As Integer, labelText As String, valueLabel As Label)
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))

        Dim label As New Label With {
            .Text = labelText,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleRight,
            .Padding = New Padding(0, 0, 12, 0),
            .ForeColor = Color.FromArgb(0, 0, 0),
            .Font = New Font(Font, FontStyle.Bold)
        }

        valueLabel.Dock = DockStyle.Fill
        valueLabel.TextAlign = ContentAlignment.MiddleLeft
        valueLabel.ForeColor = Color.FromArgb(0, 0, 0)
        valueLabel.AutoEllipsis = True

        layout.Controls.Add(label, 0, row)
        layout.Controls.Add(valueLabel, 1, row)
    End Sub

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

            Dim branchCode As Integer? = policy.RamoCodigo
            If Not branchCode.HasValue Then
                branchCode = selectedBranch.Codigo
            End If

            _ramoValue.Text = FormatCodeDescription(branchCode, policy.Ramo)
            _numeroValue.Text = policy.NumeroPoliza
            _certificadoValue.Text = If(policy.NumeroCertificado.HasValue, policy.NumeroCertificado.Value.ToString(), certificateNumber.ToString())
            _productoValue.Text = FormatCodeDescription(policy.ProductoCodigo, policy.Producto)
            _estadoValue.Text = policy.Estado
            _aseguradoValue.Text = policy.Asegurado
            _desdeValue.Text = FormatDate(policy.VigenciaDesde)
            _hastaValue.Text = FormatDate(policy.VigenciaHasta)
            _clienteValue.Text = policy.NumeroCliente
            _coberturaValue.Text = If(policy.CoberturaPrincipal.HasValue, policy.CoberturaPrincipal.Value.ToString(), "")

            SetStatus("Consulta realizada.")
        Catch ex As Exception
            SetStatus(ex.Message, True)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Sub SetBusy(isBusy As Boolean, Optional message As String = "")
        UseWaitCursor = isBusy
        _branchComboBox.Enabled = Not isBusy AndAlso _branchComboBox.Items.Count > 0
        _policyNumberTextBox.Enabled = Not isBusy
        _certificateNumeric.Enabled = Not isBusy
        _effectiveDatePicker.Enabled = Not isBusy
        _searchButton.Enabled = Not isBusy AndAlso _branchComboBox.Items.Count > 0

        If Not String.IsNullOrWhiteSpace(message) Then
            SetStatus(message)
        End If
    End Sub

    Private Sub SetStatus(message As String, Optional isError As Boolean = False)
        _statusLabel.Text = message
        _statusLabel.ForeColor = If(
            isError,
            Color.Firebrick,
            If(String.IsNullOrWhiteSpace(message), Color.DimGray, Color.DarkGreen))
    End Sub

    Private Sub ClearResult()
        _ramoValue.Text = ""
        _numeroValue.Text = ""
        _certificadoValue.Text = ""
        _productoValue.Text = ""
        _estadoValue.Text = ""
        _aseguradoValue.Text = ""
        _desdeValue.Text = ""
        _hastaValue.Text = ""
        _clienteValue.Text = ""
        _coberturaValue.Text = ""
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

    Private Shared Function FormatDate(value As DateTime?) As String
        If Not value.HasValue Then
            Return ""
        End If

        Return value.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("es-AR"))
    End Function
End Class

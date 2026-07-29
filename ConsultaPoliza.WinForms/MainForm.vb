Imports System.Data
Imports System.Globalization

Public Class MainForm
    Inherits Form

    Private Const NodePolicy As String = "policy"
    Private Const NodeStatus As String = "status"
    Private Const NodeRoles As String = "roles"
    Private Const NodeRoleHolder As String = "role-holder"
    Private Const NodeRoleInsured As String = "role-insured"
    Private Const NodeRoleDirections As String = "role-directions"
    Private Const NodeIntermediaries As String = "intermediaries"
    Private Const NodeDirectDebits As String = "direct-debits"
    Private Const NodeCoverages As String = "coverages"
    Private Const NodeCoverage As String = "coverage"
    Private Const NodeClauses As String = "clauses"
    Private Const NodeDiscounts As String = "discounts"
    Private Const NodeMovements As String = "movements"
    Private Const NodeReceipts As String = "receipts"
    Private Const NodePolicyAddress As String = "policy-address"
    Private Const NodePolicyAddressItem As String = "policy-address-item"
    Private Const NodePhones As String = "phones"

    Private ReadOnly _apiClient As ApiClient
    Private ReadOnly _branchComboBox As New ComboBox()
    Private ReadOnly _policyNumberTextBox As New TextBox()
    Private ReadOnly _certificateNumeric As New NumericUpDown()
    Private ReadOnly _effectiveDatePicker As New DateTimePicker()
    Private ReadOnly _searchButton As New Button()
    Private ReadOnly _statusLabel As New Label()
    Private ReadOnly _navigationTree As New TreeView()
    Private ReadOnly _contentPanel As New Panel()

    Private _currentPolicy As PolicyResponse

    Public Sub New(apiClient As ApiClient)
        _apiClient = apiClient
        Text = "Consulta de Poliza"
        StartPosition = FormStartPosition.CenterScreen
        BackColor = Color.White
        Font = New Font("Segoe UI", 10.0F)
        MinimumSize = New Size(980, 560)
        ClientSize = New Size(1280, 720)

        BuildLayout()
        AddHandler Shown, AddressOf MainForm_Shown
    End Sub

    Private Sub BuildLayout()
        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(10),
            .ColumnCount = 1,
            .RowCount = 3,
            .BackColor = Color.White
        }

        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 104))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 28))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

        Dim searchLayout = CreateSearchLayout()
        _statusLabel.Dock = DockStyle.Fill
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft
        _statusLabel.ForeColor = Color.DimGray
        _statusLabel.AutoEllipsis = True

        ConfigureNavigationTree()
        _contentPanel.Dock = DockStyle.Fill
        _contentPanel.AutoScroll = True
        _contentPanel.BackColor = Color.White

        Dim split As New SplitContainer With {
            .Dock = DockStyle.Fill,
            .FixedPanel = FixedPanel.Panel1,
            .BackColor = Color.White
        }

        AddHandler split.SizeChanged, AddressOf ResultsSplit_SizeChanged
        split.Panel1.Padding = New Padding(0, 0, 8, 0)
        split.Panel1.Controls.Add(_navigationTree)
        split.Panel2.Controls.Add(_contentPanel)

        root.Controls.Add(searchLayout, 0, 0)
        root.Controls.Add(_statusLabel, 0, 1)
        root.Controls.Add(split, 0, 2)

        Controls.Add(root)
        AcceptButton = _searchButton

        InitializeNavigationTree(Nothing)
        ShowNoPolicySelected()
    End Sub

    Private Sub ResultsSplit_SizeChanged(sender As Object, e As EventArgs)
        Dim split = DirectCast(sender, SplitContainer)
        If split.Width <= 0 Then
            Return
        End If

        Dim desiredDistance = Math.Min(300, Math.Max(80, split.Width \ 3))

        If split.SplitterDistance <> desiredDistance Then
            split.SplitterDistance = desiredDistance
        End If
    End Sub

    Private Function CreateSearchLayout() As TableLayoutPanel
        Dim searchLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 4,
            .RowCount = 3,
            .Margin = New Padding(0)
        }

        searchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130))
        searchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        searchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160))
        searchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        searchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
        searchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
        searchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 36))

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
            .Margin = New Padding(0),
            .Padding = New Padding(0, 2, 0, 0)
        }
        actions.Controls.Add(_searchButton)
        searchLayout.Controls.Add(actions, 0, 2)
        searchLayout.SetColumnSpan(actions, 4)

        Return searchLayout
    End Function

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
        _searchButton.Text = "OK"
        _searchButton.Size = New Size(76, 30)
        _searchButton.Enabled = False
        AddHandler _searchButton.Click, AddressOf SearchButton_Click
    End Sub

    Private Sub ConfigureNavigationTree()
        _navigationTree.Dock = DockStyle.Fill
        _navigationTree.BorderStyle = BorderStyle.FixedSingle
        _navigationTree.HideSelection = False
        _navigationTree.HotTracking = True
        _navigationTree.ShowLines = True
        _navigationTree.ShowPlusMinus = True
        _navigationTree.ShowRootLines = True
        AddHandler _navigationTree.AfterSelect, AddressOf NavigationTree_AfterSelect
    End Sub

    Private Sub InitializeNavigationTree(policy As PolicyResponse)
        _navigationTree.BeginUpdate()
        _navigationTree.Nodes.Clear()

        Dim policyNode = CreateNode("Poliza", NodePolicy)
        policyNode.Nodes.Add(CreateNode("Estado", NodeStatus))

        Dim rolesNode = CreateNode("Roles", NodeRoles)

        If policy IsNot Nothing AndAlso policy.Roles IsNot Nothing AndAlso policy.Roles.Count > 0 Then
            For Each role In policy.Roles
                Dim roleNode = CreateNode(BuildRoleNodeText(role), NodeRoles)
                roleNode.Nodes.Add(CreateNode("Direcciones", NodeRoleDirections))
                rolesNode.Nodes.Add(roleNode)
            Next
        Else
            Dim holderNode = CreateNode("Rol 1 - Contratante", NodeRoleHolder)
            holderNode.Nodes.Add(CreateNode("Direcciones", NodeRoleDirections))
            rolesNode.Nodes.Add(holderNode)
            rolesNode.Nodes.Add(CreateNode("Rol 2 - Asegurado", NodeRoleInsured))
        End If

        policyNode.Nodes.Add(rolesNode)

        policyNode.Nodes.Add(CreateNode("Intermediarios", NodeIntermediaries))
        policyNode.Nodes.Add(CreateNode("Debitos directos", NodeDirectDebits))

        Dim coveragesNode = CreateNode("Coberturas", NodeCoverages)
        If policy IsNot Nothing AndAlso policy.CoberturaPrincipal.HasValue Then
            coveragesNode.Nodes.Add(CreateNode("Cobertura " & policy.CoberturaPrincipal.Value.ToString(), NodeCoverage))
        End If
        policyNode.Nodes.Add(coveragesNode)

        policyNode.Nodes.Add(CreateNode("Clausulas", NodeClauses))
        policyNode.Nodes.Add(CreateNode("Descuentos / Recargos", NodeDiscounts))
        policyNode.Nodes.Add(CreateNode("Movimientos historicos", NodeMovements))
        policyNode.Nodes.Add(CreateNode("Recibos", NodeReceipts))

        Dim addressNode = CreateNode("Direccion de poliza", NodePolicyAddress)
        Dim addressItemNode = CreateNode("Direccion de poliza 1", NodePolicyAddressItem)
        addressItemNode.Nodes.Add(CreateNode("Telefonos", NodePhones))
        addressNode.Nodes.Add(addressItemNode)
        policyNode.Nodes.Add(addressNode)

        _navigationTree.Nodes.Add(policyNode)
        policyNode.Expand()
        _navigationTree.Enabled = policy IsNot Nothing
        _navigationTree.EndUpdate()

        If policy IsNot Nothing Then
            _navigationTree.SelectedNode = policyNode
        End If
    End Sub

    Private Shared Function CreateNode(text As String, tag As String) As TreeNode
        Return New TreeNode(text) With {.Tag = tag}
    End Function

    Private Shared Function BuildRoleNodeText(role As PolicyRoleResponse) As String
        If role Is Nothing OrElse Not HasText(role.Rol) Then
            Return "Rol"
        End If

        Return "Rol " & role.Rol.Trim()
    End Function

    Private Shared Function CreateInputLabel(text As String) As Label
        Return New Label With {
            .Dock = DockStyle.Fill,
            .Text = text,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Margin = New Padding(0)
        }
    End Function

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
            InitializeNavigationTree(policy)
            ShowPolicyOverview()
            SetStatus("Consulta realizada.")
        Catch ex As Exception
            SetStatus(ex.Message, True)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Sub NavigationTree_AfterSelect(sender As Object, e As TreeViewEventArgs)
        If _currentPolicy Is Nothing OrElse e.Node Is Nothing Then
            ShowNoPolicySelected()
            Return
        End If

        Dim nodeTag = TryCast(e.Node.Tag, String)
        Select Case nodeTag
            Case NodePolicy
                ShowPolicyOverview()
            Case NodeStatus
                ShowStatusSection()
            Case NodeRoles, NodeRoleHolder, NodeRoleInsured
                ShowRolesSection()
            Case NodeCoverages, NodeCoverage
                ShowCoveragesSection()
            Case NodeRoleDirections
                ShowRoleDirectionsSection(e.Node)
            Case NodeIntermediaries, NodeDirectDebits, NodeClauses, NodeDiscounts,
                 NodeMovements, NodeReceipts, NodePolicyAddress,
                 NodePolicyAddressItem, NodePhones
                ShowUnavailableSection(e.Node.Text)
            Case Else
                ShowPolicyOverview()
        End Select
    End Sub

    Private Sub ShowPolicyOverview()
        If _currentPolicy Is Nothing Then
            ShowNoPolicySelected()
            Return
        End If

        Dim section = CreateSectionLayout("Poliza")
        Dim detailLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .ColumnCount = 2,
            .RowCount = 12,
            .Padding = New Padding(0, 8, 0, 0),
            .Margin = New Padding(0),
            .BackColor = Color.White
        }

        detailLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260))
        detailLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))

        AddDetailRow(detailLayout, 0, "Ramo", FormatCodeDescription(_currentPolicy.RamoCodigo, _currentPolicy.Ramo))
        AddDetailRow(detailLayout, 1, "Poliza", _currentPolicy.NumeroPoliza)
        AddDetailRow(detailLayout, 2, "Certificado", FormatNullableInteger(_currentPolicy.NumeroCertificado))
        AddDetailRow(detailLayout, 3, "Producto", FormatCodeDescription(_currentPolicy.ProductoCodigo, _currentPolicy.Producto))
        AddDetailRow(detailLayout, 4, "Estado", _currentPolicy.Estado)
        AddDetailRow(detailLayout, 5, "Asegurado", _currentPolicy.Asegurado)
        AddDetailRow(detailLayout, 6, "Inicio de vigencia", FormatDate(_currentPolicy.VigenciaDesde))
        AddDetailRow(detailLayout, 7, "Fin de vigencia", FormatDate(_currentPolicy.VigenciaHasta))
        AddDetailRow(detailLayout, 8, "Frecuencia de pago", _currentPolicy.FrecuenciaPago)
        AddDetailRow(detailLayout, 9, "Numero cliente", _currentPolicy.NumeroCliente)
        AddDetailRow(detailLayout, 10, "Cobertura principal", FormatNullableInteger(_currentPolicy.CoberturaPrincipal))
        AddDetailRow(detailLayout, 11, "Fecha de efecto", FormatDate(_currentPolicy.FechaEfecto))

        section.Controls.Add(detailLayout, 0, 1)
        SetContent(section)
    End Sub

    Private Sub ShowStatusSection()
        Dim rows As New List(Of String())()
        Dim status = _currentPolicy.EstadoDetalle

        If status IsNot Nothing AndAlso (
            HasText(status.Estado) OrElse
            HasText(status.MotivoAnulacion) OrElse
            status.FechaEfectivaAnulacion.HasValue OrElse
            HasText(status.MotivoSuspension)) Then

            rows.Add(New String() {
                If(status.Estado, "").Trim(),
                If(status.MotivoAnulacion, "").Trim(),
                FormatDate(status.FechaEfectivaAnulacion),
                If(status.MotivoSuspension, "").Trim()
            })
        End If

        ShowGridSection(
            "Estado",
            New String() {"Estado", "Motivo anulacion", "Fecha efectiva anulacion", "Motivo suspension"},
            rows)
    End Sub

    Private Sub ShowRolesSection()
        Dim rows As New List(Of String())()

        If _currentPolicy.Roles IsNot Nothing Then
            For Each role In _currentPolicy.Roles
                If role Is Nothing Then
                    Continue For
                End If

                rows.Add(New String() {
                    If(role.Rol, "").Trim(),
                    If(role.Cliente, "").Trim(),
                    FormatDate(role.FechaAnulacion),
                    FormatDate(role.FechaEfecto)
                })
            Next
        End If

        ShowGridSection(
            "Roles",
            New String() {"Rol", "Cliente", "Fecha de anulacion", "Fecha de efecto"},
            rows)
    End Sub

    Private Sub ShowRoleDirectionsSection(node As TreeNode)
        Dim role = FindRoleForNode(node)
        Dim rows As New List(Of String())()

        If role IsNot Nothing AndAlso role.Direcciones IsNot Nothing Then
            For Each address In role.Direcciones
                If address Is Nothing Then
                    Continue For
                End If

                rows.Add(New String() {
                    If(address.Tipo, "").Trim(),
                    If(address.Direccion, "").Trim(),
                    If(address.CodigoPostal, "").Trim(),
                    If(address.Localidad, "").Trim(),
                    If(address.Provincia, "").Trim(),
                    If(address.Pais, "").Trim(),
                    If(address.Email, "").Trim()
                })
            Next
        End If

        ShowGridSection(
            "Direcciones",
            New String() {"Tipo", "Direccion", "Codigo Postal", "Localidad", "Provincia", "Pais", "Email"},
            rows)
    End Sub

    Private Sub ShowCoveragesSection()
        Dim rows As New List(Of String())()

        If _currentPolicy.CoberturaPrincipal.HasValue Then
            rows.Add(New String() {
                FormatCodeDescription(_currentPolicy.ProductoCodigo, _currentPolicy.Producto),
                "Cobertura " & _currentPolicy.CoberturaPrincipal.Value.ToString(),
                "",
                "",
                "",
                "",
                ""
            })
        End If

        ShowGridSection(
            "Coberturas",
            New String() {"Modulo", "Cobertura", "Moneda", "Capital asegurado", "Tasa anual", "Prima anual", "Tipo indice revaluacion"},
            rows)
    End Sub

    Private Sub ShowUnavailableSection(title As String)
        Dim section = CreateSectionLayout(title)
        section.Controls.Add(CreateEmptyStateLabel("No hay datos disponibles para esta seccion con la respuesta actual de la API."), 0, 1)
        SetContent(section)
    End Sub

    Private Sub ShowNoPolicySelected()
        Dim section = CreateSectionLayout("Poliza")
        section.Controls.Add(CreateEmptyStateLabel("Ingrese una poliza y presione OK para ver el menu lateral."), 0, 1)
        SetContent(section)
    End Sub

    Private Sub ShowGridSection(title As String, columns As String(), rows As List(Of String()))
        If rows.Count = 0 Then
            ShowUnavailableSection(title)
            Return
        End If

        Dim section = CreateSectionLayout(title)
        Dim grid As New DataGridView With {
            .Dock = DockStyle.Fill,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            .ReadOnly = True,
            .RowHeadersVisible = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
        }

        For Each columnName In columns
            grid.Columns.Add(New DataGridViewTextBoxColumn With {
                .HeaderText = columnName,
                .Name = columnName,
                .SortMode = DataGridViewColumnSortMode.NotSortable
            })
        Next

        For Each row In rows
            Dim values(row.Length - 1) As Object
            Array.Copy(row, values, row.Length)
            grid.Rows.Add(values)
        Next

        section.Controls.Add(grid, 0, 1)
        SetContent(section)
    End Sub

    Private Function CreateSectionLayout(title As String) As TableLayoutPanel
        Dim section As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .Margin = New Padding(0),
            .BackColor = Color.White
        }

        section.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))
        section.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

        Dim header As New Label With {
            .Dock = DockStyle.Fill,
            .Text = title,
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.FromArgb(232, 236, 242),
            .ForeColor = Color.FromArgb(35, 42, 52),
            .Font = New Font(Font, FontStyle.Bold),
            .BorderStyle = BorderStyle.FixedSingle
        }

        section.Controls.Add(header, 0, 0)
        Return section
    End Function

    Private Sub AddDetailRow(layout As TableLayoutPanel, row As Integer, labelText As String, valueText As String)
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))

        Dim label As New Label With {
            .Text = labelText,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleRight,
            .Padding = New Padding(0, 0, 12, 0),
            .ForeColor = Color.FromArgb(0, 0, 0),
            .Font = New Font(Font, FontStyle.Bold)
        }

        Dim valueLabel As New Label With {
            .Text = If(valueText, ""),
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .ForeColor = Color.FromArgb(0, 0, 0),
            .AutoEllipsis = True
        }

        layout.Controls.Add(label, 0, row)
        layout.Controls.Add(valueLabel, 1, row)
    End Sub

    Private Shared Function CreateEmptyStateLabel(text As String) As Label
        Return New Label With {
            .Dock = DockStyle.Fill,
            .Text = text,
            .TextAlign = ContentAlignment.MiddleCenter,
            .ForeColor = Color.DimGray,
            .BackColor = Color.White,
            .AutoEllipsis = True
        }
    End Function

    Private Sub SetContent(control As Control)
        For index = _contentPanel.Controls.Count - 1 To 0 Step -1
            Dim existingControl = _contentPanel.Controls(index)
            _contentPanel.Controls.RemoveAt(index)
            existingControl.Dispose()
        Next

        _contentPanel.Controls.Add(control)
    End Sub

    Private Sub SetBusy(isBusy As Boolean, Optional message As String = "")
        UseWaitCursor = isBusy
        _branchComboBox.Enabled = Not isBusy AndAlso _branchComboBox.Items.Count > 0
        _policyNumberTextBox.Enabled = Not isBusy
        _certificateNumeric.Enabled = Not isBusy
        _effectiveDatePicker.Enabled = Not isBusy
        _searchButton.Enabled = Not isBusy AndAlso _branchComboBox.Items.Count > 0
        _navigationTree.Enabled = Not isBusy AndAlso _currentPolicy IsNot Nothing

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

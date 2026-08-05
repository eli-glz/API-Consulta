Imports System.Data
Imports System.Globalization

Partial Public Class MainForm
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
            Case NodeReceipts
                _receiptsPageIndex = 0
                ShowReceiptsSection()
            Case NodeReceipt
                ShowReceiptsSection(e.Node.Index)
            Case NodeIntermediaries, NodeDirectDebits, NodeClauses,
                NodeDiscounts, NodeMovements, NodePolicyAddress,
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
        SetExportData("Poliza", BuildPolicyExportTable())
    End Sub

    Private Function BuildPolicyExportTable() As DataTable
        Dim table As New DataTable("Poliza")

        table.Columns.Add("Ramo")
        table.Columns.Add("Poliza")
        table.Columns.Add("Certificado")
        table.Columns.Add("Producto")
        table.Columns.Add("Estado")
        table.Columns.Add("Asegurado")
        table.Columns.Add("Inicio de vigencia")
        table.Columns.Add("Fin de vigencia")
        table.Columns.Add("Frecuencia de pago")
        table.Columns.Add("Numero cliente")
        table.Columns.Add("Cobertura principal")
        table.Columns.Add("Fecha de efecto")

        table.Rows.Add(
            FormatCodeDescription(_currentPolicy.RamoCodigo, _currentPolicy.Ramo),
            _currentPolicy.NumeroPoliza,
            FormatNullableInteger(_currentPolicy.NumeroCertificado),
            FormatCodeDescription(_currentPolicy.ProductoCodigo, _currentPolicy.Producto),
            _currentPolicy.Estado,
            _currentPolicy.Asegurado,
            FormatDate(_currentPolicy.VigenciaDesde),
            FormatDate(_currentPolicy.VigenciaHasta),
            _currentPolicy.FrecuenciaPago,
            _currentPolicy.NumeroCliente,
            FormatNullableInteger(_currentPolicy.CoberturaPrincipal),
            FormatDate(_currentPolicy.FechaEfecto))

        Return table
    End Function

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
End Class
Partial Public Class MainForm
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
        split.Panel2.Controls.Add(CreateResultsLayout())

        root.Controls.Add(searchLayout, 0, 0)
        root.Controls.Add(_statusLabel, 0, 1)
        root.Controls.Add(split, 0, 2)

        Controls.Add(root)
        AcceptButton = _searchButton

        InitializeNavigationTree(Nothing)
        ShowNoPolicySelected()
    End Sub

    Private Function CreateResultsLayout() As TableLayoutPanel
        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .Margin = New Padding(0),
            .BackColor = Color.White
        }

        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 50))

        Dim exportHost As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White
        }

        Dim exportBar As New FlowLayoutPanel With {
            .Dock = DockStyle.Right,
            .AutoSize = True,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Padding = New Padding(0, 5, 0, 0)
        }

        Dim exportLabel As New Label With {
            .Text = "Exportar a:",
            .AutoSize = True,
            .Margin = New Padding(0, 5, 8, 0)
        }

        ConfigureExportButton(
            _csvExportButton,
            "CSV",
            "Exportar a CSV",
            AddressOf CsvExportButton_Click)

        ConfigureExportButton(
            _excelExportButton,
            "XLSX",
            "Exportar a Excel",
            AddressOf ExcelExportButton_Click)

        ConfigureExportButton(
            _pdfExportButton,
            "PDF",
            "Exportar a PDF",
            AddressOf PdfExportButton_Click)

        ConfigureExportButton(
            _rtfExportButton,
            "RTF",
            "Exportar a RTF",
            AddressOf RtfExportButton_Click)

        exportBar.Controls.Add(exportLabel)
        exportBar.Controls.Add(_csvExportButton)
        exportBar.Controls.Add(_excelExportButton)
        exportBar.Controls.Add(_pdfExportButton)
        exportBar.Controls.Add(_rtfExportButton)

        exportHost.Controls.Add(exportBar)
        layout.Controls.Add(_contentPanel, 0, 0)
        layout.Controls.Add(exportHost, 0, 1)

        Return layout
    End Function

    Private Sub ConfigureExportButton(
        button As Button,
        text As String,
        toolTip As String,
        clickHandler As EventHandler)

        button.Text = text
        button.Size = New Size(64, 36)
        button.TextAlign = ContentAlignment.MiddleCenter
        button.Enabled = False
        button.Margin = New Padding(2)

        _exportToolTip.SetToolTip(button, toolTip)
        AddHandler button.Click, clickHandler
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
End Class
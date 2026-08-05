Partial Public Class MainForm
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
    Private Const NodeReceipt As String = "receipt"
    Private Const ReceiptsPageSize As Integer = 10
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
    Private ReadOnly _exportToolTip As New ToolTip()
    Private ReadOnly _csvExportButton As New Button()
    Private ReadOnly _excelExportButton As New Button()
    Private ReadOnly _pdfExportButton As New Button()
    Private ReadOnly _rtfExportButton As New Button()

    Private _currentPolicy As PolicyResponse
    Private _currentExportTitle As String = ""
    Private _currentExportTable As DataTable
    Private _currentExportGrid As DataGridView
    Private _receiptsPageIndex As Integer

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
End Class

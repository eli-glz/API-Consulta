Friend Module Program

    <STAThread()>
    Friend Sub Main(args As String())
        Application.SetHighDpiMode(HighDpiMode.SystemAware)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Using apiClient As New ApiClient("http://localhost:5045")
            Application.Run(New MainForm(apiClient))
        End Using
    End Sub

End Module

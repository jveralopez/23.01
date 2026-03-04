On Error Resume Next

Sub Trace(level, msg)
	Dim fso, f
	
	Set fso = CreateObject("Scripting.FileSystemObject")
	Set f = fso.OpenTextFile("..\install_2.log", 8, True, 0)
	Call f.Write(Right("00" & Day(Now),2) & "/" & Right("00" & Month(Now),2) & "/" & Right("0000" & Year(Now),4) & " " & Right("00" & Hour(Now),2) & ":" & Right("00" & Minute(Now),2) & ":" & Right("00" & Second(Now),2) & " - <" & level & "> - " & msg & VbCrLf)
	Call f.Close()
End Sub
Call Trace("INF","***********************************************")
Call Trace("INF","* START SERVICES (NomadService)               *")
Call Trace("INF","***********************************************")

Set objWMIService = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")
if (Err.Number<>0) then Call Trace("ERR","GetObject - FAILED - "+Err.Description)

Set colServiceList = objWMIService.ExecQuery("Select * from Win32_Service where Name='nomadservice'")
if (Err.Number<>0) then Call Trace("ERR","objWMIService.ExecQuery - FAILED - "+Err.Description)

For each objService in colServiceList
	Call Trace("INF","Start Service: " & objService.Name)
	Call objService.StartService()
Next

Call Trace("INF","***********************************************")

